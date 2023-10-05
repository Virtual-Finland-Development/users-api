# Database instructions

## Reverting to a specific RDS snapshot

Manually reverting to a RDS snapshot means creating a new database and updating the app references to that. For pulumi this means updating the stack with the new snapshot identifier.

1. Go to AWS Console and select RDS
2. Select the database instance
3. Select the tab "Maintenance & backups" and navigate to the "Snapshots"
4. Select the snapshot you want to revert to
5. Copy the snapshot ARN (identifier), e.g. `arn:aws:rds:eu-north-1:1234567890:snapshot:initial-backup-test`
6. With a code editor: update the pulumi stack with the new snapshot identifier:

```bash

// Update the RDS instance to use the manually created snapshot
var instanceName = "keep-the-same-name-as-before";
var rdsPostGreInstance = new Instance(instanceName, new InstanceArgs()
{
  // ... other configuration options
  SnapshotIdentifier = snapshotArn, // eg. "arn:aws:rds:eu-north-1:1234567890:snapshot:initial-backup-test",
});

```

7. Run `pulumi up` to update the stack (depending on the config the database might be destroyed and recreated, or just updated with the snapshot, check the output of the `pulumi up` command to see what will happen before confirming the update)
8. Ensure that the stack is updated with the newly created database and that the data reflects the snapshot
9. Remove the `SnapshotIdentifier` from the pulumi code and run `pulumi up` again to remove the reference to the snapshot

```bash
var instanceName = "keep-the-same-name-as-before";
var rdsPostGreInstance = new Instance(instanceName, new InstanceArgs()
{
  // ... other configuration options
});
```

10. Ensure the database works as expected

## Running migrations manually

1. Resolve the pulumi stack output for the database migrator lambda function ARN: `AdminFunctionArn`
2. Invoke the function:

- with AWS CLI: `aws lambda invoke --payload '{"action": "migrate"}' --cli-binary-format raw-in-base64-out --function-name <AdminFunctionArn> output.json`
- or by using the AWS Console:
  - Go to AWS Console and select Lambda
  - Select the function with name like `users-api-AdminFunction-<stage>-<random-hash>`
  - Select the tab "Test"
  - Add a new test event with the following content: `{"action": "migrate"}`
  - Click "Test" button and wait for the execution to finish
