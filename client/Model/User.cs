using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

namespace client.Model
{
    class User
    {
        private int userid;
        private AGECATEGORY agecat;
        private String username;

        private List<int> friendIDList;
    }
}
