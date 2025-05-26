# BootstrapBlazor.Copilot.MCPServer

**中文 README** | [English README](README.md)

一个基于 Model Context Protocol (MCP) 的服务器，为 AI 助手（如 GitHub Copilot、Claude）提供 BootstrapBlazor 组件的文档和源代码访问能力。

## 项目概述

BootstrapBlazor.Copilot.MCPServer 是一个专门为 BootstrapBlazor 框架设计的 MCP 服务器，它可以：

- 📚 提供 BootstrapBlazor 组件的完整文档
- 🔍 允许 AI 助手搜索和浏览组件源代码
- 📖 提供组件使用示例和最佳实践
- 🔄 自动同步最新的 BootstrapBlazor 源代码
- 🛠️ 支持通过 HTTP 传输与 AI 助手集成

## 架构设计

项目采用 .NET 9.0 和 ASP.NET Core 构建，包含以下主要组件：

### 项目结构

```
BootstrapBlazor.Copilot.MCPServer/
├── BootstrapBlazor.Copilot.MCPServer/           # 主服务项目
│   ├── Services/                                # 核心服务
│   │   ├── GitRepositoryManager.cs             # Git 仓库管理
│   │   └── ComponentDocumentationService.cs    # 组件文档服务
│   ├── Tools/                                   # MCP 工具定义
│   │   └── ComponentsTool.cs                   # 组件查询工具
│   ├── Models/                                  # 数据模型
│   │   └── Component.cs                        # 组件相关模型
│   └── Program.cs                              # 应用入口点
├── BootstrapBlazor.Copilot.MCPServer.AppHost/  # Aspire 应用主机
└── BootstrapBlazor.Copilot.MCPServer.ServiceDefaults/  # 共享服务配置
```

### 核心功能

1. **Git 仓库管理** (`GitRepositoryManager`)
   - 自动克隆和更新 BootstrapBlazor 官方仓库
   - 检查本地仓库是否为最新版本
   - 后台服务定期同步代码

2. **组件文档服务** (`ComponentDocumentationService`)
   - 扫描并索引所有 BootstrapBlazor 组件
   - 提供组件源代码和示例文件访问
   - 智能路径解析，支持多种文件组织结构

3. **MCP 工具集** (`ComponentsTool`)
   - `ListComponents`: 列出所有可用组件
   - `GetComponentFiles`: 获取组件文件列表
   - `GetFileContent`: 获取具体文件内容
   - `Echo`: 测试工具

## 技术栈

- **.NET 9.0**: 最新的 .NET 框架
- **ASP.NET Core**: Web 应用框架
- **Model Context Protocol**: AI 助手集成协议
- **LibGit2Sharp**: Git 操作库
- **.NET Aspire**: 云原生应用开发平台
- **OpenTelemetry**: 可观测性和监控

## 安装和配置

### 前置要求

- .NET 9.0 SDK
- Git
- Visual Studio 2022 或 VS Code

### 快速开始

1. **克隆项目**
   ```bash
   git clone https://github.com/your-repo/BootstrapBlazor.Copilot.MCPServer.git
   cd BootstrapBlazor.Copilot.MCPServer
   ```

2. **恢复依赖**
   ```bash
   dotnet restore
   ```

3. **运行服务**
   ```bash
   # 使用 Aspire（推荐）
   dotnet run --project BootstrapBlazor.Copilot.MCPServer.AppHost

   # 或直接运行主项目
   dotnet run --project BootstrapBlazor.Copilot.MCPServer
   ```

4. **验证服务**
   服务默认运行在 `https://localhost:3001`，支持以下端点：
   - `/health` - 健康检查
   - `/alive` - 存活检查
   - MCP 协议端点

### 配置选项

在 `appsettings.json` 中可以配置以下选项：

```json
{
  "McpToolConfig": {
    "ServerUrl": "https://localhost:3001"
  },
  "GitRepository": {
    "LocalPath": "C:\\temp\\BootstrapBlazorRepo"  // 可选：自定义本地仓库路径
  }
}
```

## MCP 工具使用

### 1. 列出所有组件

```bash
# MCP 调用示例
ListComponents()
```

返回所有 BootstrapBlazor 组件的名称和描述。

### 2. 获取组件文件列表

```bash
GetComponentFiles("Button")
```

返回指定组件的源代码文件和示例文件列表。

### 3. 获取文件内容

```bash
GetFileContent({
  "ComponentName": "Button",
  "FileName": "Button.razor.cs",
  "Category": "Source"  // Source 或 Example
})
```

返回指定文件的完整内容。

## AI 助手集成

本服务设计用于与支持 MCP 协议的 AI 助手集成：

### GitHub Copilot 集成

1. 在 GitHub Copilot 设置中添加 MCP 服务器
2. 配置服务器地址：`https://localhost:3001`
3. 重启 GitHub Copilot 以应用配置

### Claude Desktop 集成

在 Claude Desktop 配置文件中添加：

```json
{
  "mcpServers": {
    "bootstrapblazor": {
      "command": "dotnet",
      "args": ["run", "--project", "path/to/BootstrapBlazor.Copilot.MCPServer"],
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Production"
      }
    }
  }
}
```

## 开发指南

### 添加新的 MCP 工具

1. 在 `Tools` 目录下创建新的工具类
2. 使用 `[McpServerToolType]` 标记类
3. 使用 `[McpServerTool]` 标记工具方法
4. 在 `Program.cs` 中注册服务

示例：

```csharp
[McpServerToolType]
public class NewTool
{
    [McpServerTool, Description("新工具描述")]
    public string NewMethod(string parameter)
    {
        return "结果";
    }
}
```

### 扩展组件分析

在 `ComponentDocumentationService` 中可以添加新的组件分析逻辑：

- 解析组件属性
- 提取使用示例
- 生成组件关系图

## 监控和运维

### 健康检查

- `/health` - 完整健康检查
- `/alive` - 基础存活检查

### 日志记录

项目使用 .NET 标准日志框架，支持：
- 控制台输出
- 文件日志（需配置）
- 结构化日志
- OpenTelemetry 集成

### 性能监控

通过 .NET Aspire 和 OpenTelemetry 提供：
- 请求追踪
- 性能指标
- 分布式追踪
- 错误监控

## 故障排除

### 常见问题

1. **Git 仓库克隆失败**
   - 检查网络连接
   - 验证 Git 是否正确安装
   - 检查防火墙设置

2. **MCP 连接失败**
   - 验证服务器地址配置
   - 检查端口是否被占用
   - 确认 MCP 客户端配置正确

3. **组件文件未找到**
   - 确认 BootstrapBlazor 仓库已正确克隆
   - 检查本地仓库路径配置
   - 验证组件名称拼写

### 调试模式

开发环境下启用详细日志：

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "BootstrapBlazor.Copilot.MCPServer": "Trace"
    }
  }
}
```

## 贡献指南

1. Fork 本项目
2. 创建功能分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 创建 Pull Request

## 许可证

本项目采用 Apache License 2.0 许可证。详细信息请参阅 [LICENSE](LICENSE.txt) 文件。

## 相关链接

- [BootstrapBlazor 官方网站](https://www.blazor.zone/)
- [BootstrapBlazor GitHub 仓库](https://github.com/dotnetcore/BootstrapBlazor)
- [Model Context Protocol 规范](https://modelcontextprotocol.io/)
- [.NET Aspire 文档](https://learn.microsoft.com/en-us/dotnet/aspire/)

## 版本历史

### v1.0.0
- 初始版本发布
- 基础 MCP 工具实现
- Git 仓库自动同步
- 组件文档服务

---

**注意**: 本项目目前处于开发阶段，API 可能会发生变化。建议在生产环境使用前进行充分测试。