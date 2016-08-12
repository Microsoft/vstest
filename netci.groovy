// Import the utility functionality.
import jobs.generation.Utilities;
import jobs.generation.InternalUtilities;

// Defines a the new of the repo, used elsewhere in the file
def project = GithubProject

// Generate the builds for debug and release, commit and PRJob
[true, false].each { isPR -> // Defines a closure over true and false, value assigned to isPR
    ['Debug', 'Release'].each { configuration ->
        
        // Determine the name for the new job. A _prtest suffix is appended if isPR is true.
        def newJobName = InternalUtilities.getFullJobName(project, configuration, isPR)
        
        // Define your build/test strings here
        def buildString = """call build.cmd -c ${configuration}"""

        // Create a new job for windows build
        def newJob = job(newJobName) {
            steps {
                batchFile(buildString)
            }
        }

        Utilities.setMachineAffinity(newJob, 'Windows_NT', 'latest-or-auto-internal')
        
        // This call performs remaining common job setup on the newly created job.
        InternalUtilities.standardJobSetup(newJob, project, isPR, "*/${branchName}")

        if (isPR) {
            Utilities.addGithubPRTriggerForBranch(newJob, branch, "Windows / ${os} ${configuration} Build")
        }
        else {
            Utilities.addGithubPushTrigger(newJob)
        }
    }
}
