## AfimilkTask

## Overview
This project implements a **Job Scheduler** that supports executing C# code jobs (under the **System** namespace). Jobs can be scheduled for repeated or limited occurrences, and all job output appears in the server console.

## Ways to Test the Job Scheduler

### 1. **Unit Tests**
To run the unit tests:
- Right-click on the `JobSchedulerTests` project and select **Run Tests**.

The unit tests include:
- **Internal Component Tests**: Verifies `JobSchedulerService` and `JobRepository` functionality.
- **System Tests**: Starts a server instance and uses an HTTP client to interact with the server API.

### 2. **Demo API**
Run the `JobScheduler` project, and in **Swagger**, you’ll find the `run-demo` API. This endpoint does not require any parameters.

- **Description and Output**: The demo’s behavior and outputs are shown directly in the server process console.

### 3. **Manual Run**
To manually register new jobs, use the `register` API (with swagger, postman etc..). Here’s an sample request body you can modify:

#### Job Registration Request Example:
```json
{
  "name": "Job1",
  "executionTime": "06:35:04.7217711",
  "maxOccurrences": 2,
  "scriptCode": "Console.WriteLine(\"test job1 !!!\")"
}

```

## Additional Notes
- All job outputs will appear in the server console.
- Status Check: During execution, you can call the Jobs/all API to see the current status and execution updates for each job.

