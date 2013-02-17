using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Net;
using System.Threading;

class ServerThread
{
    public string server_status = "offline";
    public string host_id = "";
    public string revitClientIP = "";
    public string current_server_activity = "";
    WebClient client = new WebClient();
    public IServerThread main;

    public void ServerLoop()
    {
        while (server_status == "online")
        {
            NameValueCollection pa = new NameValueCollection();
            pa.Add("sid", host_id);

            byte[] data = client.UploadValues(AppConst.SERVER_DOMAIN + AppConst.SERVER_PATH + "?act=SetServerAct", pa);
            current_server_activity = HoUtility.ByteToString(data);
            main.Log("current_server_activity: " + current_server_activity);
            if (current_server_activity == "client_connected")
            {
                data = client.UploadValues(AppConst.SERVER_DOMAIN + AppConst.SERVER_PATH + "?act=ClientIP", pa);
                revitClientIP = HoUtility.ByteToString(data).Trim();
            }
            else if (current_server_activity == "model_uploaded")
            {
                System.IO.File.Delete("building.fbx");
                client.DownloadFile(AppConst.SERVER_DOMAIN + AppConst.SERVER_PATH + AppConst.SERVER_MODEL_PATH + host_id + ".fbx", "building.fbx");
                data = client.UploadValues(AppConst.SERVER_DOMAIN + AppConst.SERVER_PATH + "?act=GetSemanticFile", pa);

                main.SetSemanticInfo(HoUtility.ByteToString(data));
                main.DownloadModelCb();
            }
            Thread.Sleep(1000);
        }
    }

}
