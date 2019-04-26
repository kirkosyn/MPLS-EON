using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementCenter
{
    class Subnetwork
    {
        string address;
        List<int> linksId;
        List<int> nodeId;
        public int id;
        public string myContent;



        public Subnetwork(int id, string address, List<int> linksId, List<int> nodeId, string myContent)
        {
            this.id = id;
            this.address = address;
            this.linksId = linksId;
            this.nodeId = nodeId;
            this.myContent = myContent;


            Console.WriteLine("Stworzono podsieć o id " + id);
        }
    }
}
