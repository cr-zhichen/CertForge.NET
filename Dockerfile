# 使用 Node.js 镜像来构建前端
FROM node:16 AS node_builder
WORKDIR /app
# 定义代理变量
ARG HTTP_PROXY
ARG HTTPS_PROXY
ENV http_proxy=$HTTP_PROXY \
    https_proxy=$HTTPS_PROXY
# 安装依赖
COPY ["CertForge.NET/ClientApp/package.json", "CertForge.NET/ClientApp/package-lock.json*", "./"]
RUN npm install
COPY ["CertForge.NET/ClientApp", "."]
RUN npm run build

# 使用 .NET SDK 镜像构建后端
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
# 定义代理变量
ARG HTTP_PROXY
ARG HTTPS_PROXY
ENV http_proxy=$HTTP_PROXY \
    https_proxy=$HTTPS_PROXY
# 复制项目文件
COPY ["CertForge.NET/CertForge.NET.csproj", "."]
RUN dotnet restore "CertForge.NET.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "CertForge.NET/CertForge.NET.csproj" -c Release -o /app/build

# 发布 .NET 应用
FROM build AS publish
RUN dotnet publish "CertForge.NET/CertForge.NET.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 使用基础 ASP.NET 镜像
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 9000

# 将构建的文件复制到最终镜像中
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=node_builder /wwwroot ./wwwroot
ENTRYPOINT ["dotnet", "CertForge.NET.dll"]