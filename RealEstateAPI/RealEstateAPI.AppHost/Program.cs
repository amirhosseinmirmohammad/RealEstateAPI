var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.ElsaDashboard>("elsadashboard");

builder.Build().Run();
