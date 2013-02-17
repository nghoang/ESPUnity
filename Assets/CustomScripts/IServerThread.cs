using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

interface IServerThread
{
    void DownloadModelCb();
    void Log(string log);
    void SetSemanticInfo(string info);
}