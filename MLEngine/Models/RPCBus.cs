using SharedMemory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MLEngine.Models
{
    class RPCBus
    {
        public RpcBuffer rpc_slave;

        public string channelName;

        public delegate string GetAnswer(string channelName, string request);

        public GetAnswer get_answer;

        public RPCBus(string channelName)
        {
            this.channelName = channelName;

            rpc_slave = new RpcBuffer(channelName, (msgId, payload) =>
            {
                var request = Encoding.UTF8.GetString(payload);

                double answer = double.NaN;
                
                double.TryParse(get_answer(channelName, request), out answer);

                return Encoding.UTF8.GetBytes(answer.ToString());
            });
        }

        public void Stop()
        {
            rpc_slave.Dispose();
        }
    }
}
