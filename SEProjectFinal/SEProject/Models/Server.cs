using System;
namespace SEProject.Models
{
    public class Server
    {
        public Boolean hasCommitted { get; set; } = false;

        public Server()
        {
            this.hasCommitted = false;
        }


        public void action()
        {
            this.hasCommitted = true;
        }

    }
}
