using EmbedIO.Net;

namespace PhipseyyBot.Discord.Modules;

public class WebServereqweqe
{
    private HttpListener listener;

    public WebServereqweqe(string uri)
    {
        listener = new HttpListener();
        listener.Prefixes.Add(uri);
    }

    public async Task<Models.Authorization> Listen()
    {
        listener.Start();
        return await OnRequest();
    }

    private async Task<Models.Authorization> OnRequest()
    {
        while(listener.IsListening)
        {
            var ctx = await listener.GetContextAsync(new CancellationToken(false));
            var req = ctx.Request;
            var resp = ctx.Response;

            using (var writer = new StreamWriter(resp.OutputStream))
            {
                if (req.QueryString.AllKeys.Any("code".Contains!))
                {
                    writer.WriteLine("Authorization started! Check your application!");
                    writer.Flush();
                    return new Models.Authorization(req.QueryString["code"]);
                }
                else
                {
                    writer.WriteLine("No code found in query string!");
                    writer.Flush();
                }
            }
        }
        return null;
    }
}