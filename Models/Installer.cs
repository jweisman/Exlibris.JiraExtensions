using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExLibris.JiraExtensions.Models
{
    public class Installer
    {
        public string userName { get; set; }
        public string displayName { get; set; }
        public string location { get; set; }
        public List<Installation> openInstallations { get; set; }
        public string weekend { get; set; }
        public List<Holiday> holidays { get; set; }

        public Installer()
        {
            openInstallations = new List<Installation>();
            holidays = new List<Holiday>();
        }

    }

    public class Holiday
    {
        public DateTime date;
        public string name;
    }

    public class InstallerComparer : IEqualityComparer<Installer>
    {
        #region IEqualityComparer<Installer> Members
        bool IEqualityComparer<Installer>.Equals(Installer x, Installer y)
        {
            // Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(x, y))
                return true;

            // Check whether any of the compared objects is null.
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            return x.userName == y.userName;
        }

        int IEqualityComparer<Installer>.GetHashCode(Installer obj)
        {
            return obj.userName.GetHashCode();
        }
        #endregion
    }

}