# Database instructions

## Reverting to a specific RDS snapshot

** UNTESTED **

Manually reverting to a snapshot means creating a new database and updating the app references to that. For pulumi this means updating the stack with the new snapshot identifier.

1. Go to AWS Console and select RDS
2. Select the database instance
3. Copy the DB identifier, e.g. `users-api-postgres-db-dev123abbazebra`
4. Select the tab "Maintenance & backups" and navigate to the "Snapshots"
5. Select the snapshot you want to revert to
6. Copy the snapshot name (identifier), e.g. `pre-users-api-deployment-dev-12345678910abbazebra-47`
7. With a code editor: update the pulumi stack with the new snapshot identifier:

```bash

import * as aws from "@pulumi/aws";

// Create an RDS snapshot resource
const manualSnapshot = new aws.rds.Snapshot("manual-snapshot", {
  dbInstanceIdentifier: "<your-db-instance-id:DBidentifier>",
  dbSnapshotIdentifier: "<your-manual-snapshot-identifier:SnapshotName>",
});

// Update the RDS instance to use the manually created snapshot
const instanceName = "keep-the-same-name-as-before";
const rdsInstance = new aws.rds.Instance(instanceName, {
  // ... other configuration options
  snapshotIdentifier: manualSnapshot.id,
});

```

7. Run `pulumi up` to update the stack
8. Ensure that the stack is updated and the database is reverted to the snapshot
9. Remove the code that updates the database with snapshot:

```bash
import * as aws from "@pulumi/aws";

// Update the RDS instance
const instanceName = "keep-the-same-name-as-before";
const rdsInstance = new aws.rds.Instance(instanceName, {
  // ... other configuration options
});
```

10. Run `pulumi up` to update the stack back to the original state
