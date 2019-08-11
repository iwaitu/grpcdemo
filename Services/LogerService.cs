using Grpc.Core;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using gRpcMongoDB;
using Google.Protobuf.WellKnownTypes;
using System.Linq;
using Google.Protobuf;
using System.IO;
using System.Collections.Generic;

namespace gRpcDemo
{
    public class LogerService : Loger.LogerBase
    {
        private readonly ILogger _logger;
        private readonly LogService _service;

        public LogerService(ILogger<LogerService> logger, LogService service)
        {
            _logger = logger;
            _service = service;
        }

        public override async Task<LogReply> GetLogList(LogRequest request, ServerCallContext context)
        {
            var list = await _service.GetList(request.Date, request.Count);
            var ret = list.Select(p => new LogContent { Id = p.Id.ToString(), Date = Timestamp.FromDateTime(p.Date), Level = p.Level, Logger = p.Logger, Message = p.Message });
            return new LogReply { Items = { ret.ToArray() } };
        }



        public override async Task<LogItemReply> GetLogItem(LogItemRequest request, ServerCallContext context)
        {
            var item = await _service.GetById(request.Id);
            var content = new LogContent {Id = item.Id.ToString(),Date = Timestamp.FromDateTime(item.Date), Level = item.Level, Logger = item.Logger, Message = item.Message };
            return new LogItemReply { Item = content };
        }


        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private static ByteString GetByteString(LogReply item)
        {
            using (var ms = new MemoryStream())
            {
                item.WriteTo(ms);
                return ByteString.CopyFrom(ms.ToArray());
            }
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="byteString"></param>
        /// <returns></returns>
        private static LogReply GetPersonFromByteString(ByteString byteString)
        {
            return LogReply.Parser.ParseFrom(byteString);
        }
    }
}
