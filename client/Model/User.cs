using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace client.Model
{
    public class User
    {
        private int userid;
        private AGECATEGORY agecat;
        private SEX gender;
        private String username;
        private List<int> friendIDList;
        private bool hasOngoingChat;

        //private SEX lookingforsex;

        public User() { friendIDList = new List<int>(); }

        public int Id { get { return userid; } set { userid = value; } }

        public AGECATEGORY AgeCategory { get { return agecat; } set { agecat = value; } }

        public SEX Gender { get { return gender; } set { gender = value; } }

        //public SEX LookingForSex { get { return lookingforsex; } set { lookingforsex = value; } }

        public String Username { get { return username; } set { username = value; } }

        public List<int> FriendIDList { get { return friendIDList; } }

        public bool HasOngoingChat { get { return hasOngoingChat; } set { hasOngoingChat = value; } }
    }


}
