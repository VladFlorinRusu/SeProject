using System;
namespace SEProject.Models
{
    public class Contract
    {
        public Server server { get; set; }
        public User user { get; set; }
        public Boolean finished { get; set; }
        public int numberOfCommits;
        public int commits;


        public Contract(Server s, User cu, int com)
        {
            this.server = s;
            this.user = cu;
            this.finished = false;
            this.numberOfCommits = 0;
            this.commits = com;
        }


        public void update()
        {

            if (this.numberOfCommits < this.commits)
            {

                if (user.hasCommitted == true)
                {
                    this.numberOfCommits++;
                }
            }
            else
            {
                this.finished = true;
            }
        }


    }
}
