﻿using Bam.ServiceProxy;

namespace Bam.Data.Repositories
{
    public interface ICrudResponseProvider
    {
        DaoInfo DaoInfo { get; }
        IDaoProxyRegistration DaoProxyRegistration { get; }
        IHttpContext HttpContext { get; }

        CrudResponse Create();
        CrudResponse Delete();
        CrudResponse Execute();
        CrudResponse Query();
        CrudResponse Retrieve();
        CrudResponse SaveCollection();
        CrudResponse Update();
    }
}