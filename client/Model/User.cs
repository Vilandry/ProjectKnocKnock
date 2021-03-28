using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace knock.Model
{
    class User
    {
        private int userid;
        private AGECATEGORY agecat;
        private String username;

        private List<int> friendIDList;


        public int Id { get { return userid; } set { userid = value; } }

        public AGECATEGORY AgeCategory { get { return agecat; } set { agecat = value; } }

        public String Username { get { return username; } set { username = value; } }

        public List<int> FriendIDList { get { return friendIDList; } }
    }

    public enum AGECATEGORY
    {
        SIXTEEN,
        TWENTY,
        TWENTYFIVEPLUS
    }


    public enum SEX
    {
        FEMALE,
        MALE,
        OTHER
    }

}
