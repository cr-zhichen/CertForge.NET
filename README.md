# 🛠️ CertForge.NET

使用.NET 开发的简易自签发证书工具。

## 🚀 快速开始

1. 克隆本仓库
2. 运行 `dotnet restore`
3. 启动项目：`dotnet run`

## Docker 部署

```bash
docker run -d \
  -p 9000:9000 \
  --name certforge_dotnet \
  -e RootCertificate__C=CN \
  -e RootCertificate__O=CertForge.NET \
  -e RootCertificate__CN=CertForge.NET \
  -e ConnectionStrings__SqliteConnection="Data Source=/app/db/App.db" \
  ghcr.io/cr-zhichen/certforge.net:latest
```

如需持久化`根证书`数据，可挂载 `/app/db` 目录。
```bash
  -v /path/to/certforge_dotnet/db:/app/db \
```

## Dockerfile 部署

### 构建 Docker 镜像

``` bash
docker build --network="host" \
  --build-arg HTTP_PROXY=http://127.0.0.1:7890 \
  --build-arg HTTPS_PROXY=http://127.0.0.1:7890 \
  -t certforge_dotnet:latest .
```

若无代理需求，可省略 `--network="host"` 和 `--build-arg` 参数。

```bash
docker build --network="host" \
  -t certforge_dotnet:latest .
```

### 运行 Docker 镜像

``` bash
docker run -d \
  -p 9000:9000 \
  --name certforge_dotnet \
  -e RootCertificate__C=CN \
  -e RootCertificate__O=CertForge.NET \
  -e RootCertificate__CN=CertForge.NET \
  -e ConnectionStrings__SqliteConnection="Data Source=/app/db/App.db" \
  certforge_dotnet:latest
```

如需持久化`根证书`数据，可挂载 `/app/db` 目录。
```bash
  -v /path/to/certforge_dotnet/db:/app/db \
```
