using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using AdLoginDemo.Application.Infrastructure;

namespace AdLoginDemo.Test
{
    public class AdServiceTests
    {
        private static string _user = "";
        private static string _pass = "";

        [Fact]
        public void JsonSerializationTest()
        {
            var user = new AdUser(
                "firstname",
                "lastname",
                "email",
                "cn",
                "dn",
                new string[] { "OU=Schueler" },
                "1234");
            var user2 = AdUser.FromJson(user.ToJson());
            Assert.True(user2 is not null
                && user.Firstname == user2.Firstname
                && user.Lastname == user2.Lastname
                && user.Email == user2.Email
                && user.Cn == user2.Cn
                && user.Dn == user2.Dn
                && user.GroupMemberhips.Length == user2.GroupMemberhips.Length
                && user.PupilId == user2.PupilId);
        }

        [Fact]
        public void LoginSuccessTest()
        {
            using var adService = AdService.Login(_user, _pass);
            Assert.Contains(_user, adService.CurrentUser.Cn);
        }

        [Fact]
        public void GetRoleTest()
        {
            using var adService = AdService.Login(_user, _pass);
            Assert.True(adService.CurrentUser.Role == AdUserRole.Teacher);
        }

        [Fact]
        public void GetPupilsTest()
        {
            using var adService = AdService.Login(_user, _pass);
            var pupils = adService.GetPupils("5AHIF");
            Assert.True(pupils.Length > 0);
            Assert.True(pupils.All(p => p.Classes.Contains("5AHIF")));
        }
    }
}
