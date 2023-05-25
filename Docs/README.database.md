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
