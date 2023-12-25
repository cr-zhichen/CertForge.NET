using CertForge.NET.DTOs;
using CertForge.NET.DTOs.WebSocket.Requests;
using Newtonsoft.Json.Linq;
using Route = CertForge.NET.Enum.Route;

namespace CertForge.NET.WS;

/// <summary>
/// websocket请求处理
/// </summary>
public static class WebsocketProcess
{
    /// <summary>
    /// 处理websocket请求
    /// </summary>
    /// <param name="socket"></param>
    /// <param name="data"></param>
    public static async Task Process(UserConnection socket, WsReq data)
    {
        string routeString;
        try
        {
            routeString = data.Route ?? throw new InvalidOperationException();
        }
        catch (Exception e)
        {
            var re = new WsError<JObject>
            {
                Route = Enum.Route.Error.ToString(),
                Message = "路由请求错误，请检查路由是否正确",
                Data = JObject.FromObject(e),
            };

            JObject reObject = JObject.FromObject(re);
            await socket.SendMessageAsync(reObject);
            throw;
        }

        switch (routeString)
        {
            //测试路由
            case nameof(Enum.Route.Test):
                await socket.SendMessageAsync(JObject.FromObject(new WsOk<JObject>
                {
                    Route = Enum.Route.Test.ToString(),
                    Data = data.Data,
                }));
                break;
            //其他路由
            default:
                await socket.SendMessageAsync(JObject.FromObject(new WsError<JObject>
                {
                    Route = Enum.Route.Error.ToString(), Message = "请求路由错误，请检查路由是否正确",
                }));
                break;
        }
    }
}