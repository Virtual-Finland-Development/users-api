using VirtualFinland.UsersAPI.Deployment;

return await Pulumi.Deployment.RunAsync<UsersAPIStack>();
