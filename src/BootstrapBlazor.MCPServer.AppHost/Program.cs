// Licensed to the BootstrapBlazor Community under one or more agreements.
// The BootstrapBlazor Community licenses this file to you under the Apache 2.0 License
// See the LICENSE file in the project root for more information.
// Maintainer: Argo Zhang(argo@live.ca) Website: https://www.blazor.zone

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.BootstrapBlazor_Copilot_MCPServer>("bootstrapblazor-copilot-mcpserver");

builder.Build().Run();
