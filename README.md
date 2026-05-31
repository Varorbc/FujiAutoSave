# Fuji AutoSave

**FujiAutoSave** 用于在局域网内 **自动接收并备份富士相机通过 Wi‑Fi 发送的照片文件**，
让你无需依赖官方软件，也不必再频繁插拔相机的 SD 卡，就能在任意设备上（PC、NAS、服务器、Docker）实现 **稳定、高效、全自动的无线备份体验**。  

[![CI](https://github.com/Varorbc/FujiAutoSave/actions/workflows/ci.yml/badge.svg)](https://github.com/Varorbc/FujiAutoSave/actions/workflows/ci.yml)
[![Release](https://github.com/Varorbc/FujiAutoSave/actions/workflows/release.yml/badge.svg)](https://github.com/Varorbc/FujiAutoSave/actions/workflows/release.yml)
[![Docker Pulls](https://img.shields.io/docker/pulls/varorbc/fuji-autosave)](https://hub.docker.com/r/varorbc/fuji-autosave)

## 功能特性

- 📷 **自动发现相机**: 监听富士相机的 UDP 广播发现
- 🔗 **WiFi 连接**: 通过富士 PC AutoSave 协议连接
- 💾 **自动备份**: 自动备份相机中的照片到指定目录
- 🐳 **Docker 部署**: 支持 Docker 镜像

## 系统要求

- .NET 10.0 SDK
- 富士 X 系列相机（支持 PC AutoSave 功能）
- 相机和电脑在同一网络

## 快速开始

### 本地运行

```bash
# 克隆仓库
git clone https://github.com/Varorbc/FujiAutoSave.git
cd FujiAutoSave

# 还原依赖
dotnet restore

# 运行应用
dotnet run --project src/FujiAutoSave
```

应用程序启动后会监听 UDP 51541/51542 端口，等待相机连接。

### Docker 运行

```bash
docker run -d \
  -p 51541:51541/tcp \
  -p 51541:51541/udp \
  -p 51542:51542/tcp \
  -p 51542:51542/udp \
  -v /path/to/photos:/root/Photos \
  --name fuji-autosave \
  varorbc/fuji-autosave:latest
```

## 使用方法

1. **注册**: 在相机上进入 `设置` → `连接设定` → `PC自动保存`
2. **连接**: 在相机上进入 `播放菜单` → `PC自动保存` → 选择你的电脑名称
3. **自动备份**: 相机会自动开始传输照片到电脑

## 支持的设备

- 支持 WiFi AutoSave 功能的富士相机(目前仅测试了X-S10)
