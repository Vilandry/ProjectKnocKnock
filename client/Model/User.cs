using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace client.Model
{
    public class User
    {
        private AGECATEGORY agecat;
        private GENDER gender;
        private String username;
        private volatile bool hasOngoingChat;
        private volatile bool hasOngoingChatSearch;

        private string lastPrivateChatConversationId;
        private string lastPrivateChatUsername;
        private string lastPrivateChatHistory;


        public User() { }

        public AGECATEGORY AgeCategory { get { return agecat; } set { agecat = value; } }

        public GENDER Gender { get { return gender; } set { gender = value; } }

        public String Username { get { return username; } set { username = value; } }

        public bool HasOngoingChat { get { return hasOngoingChat; } set { hasOngoingChat = value; } }
        public bool HasOngoingChatSearch { get { return hasOngoingChatSearch; } set { hasOngoingChatSearch = value; } }

        public string LastPrivateChatConversationId { get { return lastPrivateChatConversationId; } set { lastPrivateChatConversationId = value; } }
        public string LastPrivateChatUsername { get { return lastPrivateChatUsername; } set { lastPrivateChatUsername = value; } }
        public string LastPrivateChatHistory { get { return lastPrivateChatHistory; } set { lastPrivateChatHistory = value; } }
    }


}
