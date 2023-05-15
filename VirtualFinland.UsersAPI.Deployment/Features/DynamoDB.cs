using Pulumi;
using VirtualFinland.UsersAPI.Deployment.Common.Models;
using Pulumi.Aws.DynamoDB;
using Pulumi.Aws.DynamoDB.Inputs;
using Pulumi.Aws.Iam;

namespace VirtualFinland.UsersAPI.Deployment.Features;

/// <summary>
/// Creates dynamodb cache table
/// </summary>
public class DynamoDB
{
    public DynamoDB(Config config, StackSetup stackSetup)
    {
        StackSetup = stackSetup;
        var tableName = config.Require("dynamoDbTableName");
        var table = new Table(tableName, new TableArgs
        {
            Name = tableName,
            BillingMode = "PAY_PER_REQUEST",
            HashKey = "id",
            Attributes = new TableAttributeArgs
            {
                Name = "id",
                Type = "S"
            },
            Tags = stackSetup.Tags
        });

        TableName = table.Name;
        Arn = table.Arn;
    }

    public Policy GetPolicy()
    {
        return new Policy($"{StackSetup.ProjectName}-DynamoDBAccessPolicy-{StackSetup.Environment}", new()
        {
            Description = "Users-API DynamoDB Access Policy",
            PolicyDocument = Output.Format($@"{{
                ""Version"": ""2012-10-17"",
                ""Statement"": [
                    {{
                        ""Effect"": ""Allow"",
                        ""Action"": [
                            ""dynamodb:PutItem"",
                            ""dynamodb:GetItem"",
                            ""dynamodb:DeleteItem"",
                            ""dynamodb:UpdateItem"",
                        ],
                        ""Resource"": [
                            ""{Arn}""
                        ]
                    }}
                ]
            }}"),
            Tags = StackSetup.Tags,
        });
    }

    public Output<string> TableName = default!;
    public Output<string> Arn = default!;
    private readonly StackSetup StackSetup;
}