var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.BootstrapBlazor_Copilot_MCPServer>("bootstrapblazor-copilot-mcpserver");

builder.Build().Run();
