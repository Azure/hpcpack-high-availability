// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
namespace Microsoft.Hpc.HighAvailabilityModule.Interface
{
    using System;

    public class HeartBeatEntry : IEquatable<HeartBeatEntry>
    {
        public HeartBeatEntry(string uuid, string utype, string uname, DateTime timeStamp)
        {
            this.Uuid = uuid;
            this.Utype = utype;
            this.Uname = uname;
            this.TimeStamp = timeStamp;
        }

        public string Uuid { get; }

        public string Utype { get; }

        public string Uname { get; }

        public DateTime TimeStamp { get; }

        public bool IsEmpty => string.IsNullOrEmpty(this.Uuid);

        private static string DefaultTime = "1753-01-01 12:00:00.000";

        public static HeartBeatEntry Empty { get; } = new HeartBeatEntry(string.Empty, string.Empty, string.Empty, Convert.ToDateTime(DefaultTime));

        public override string ToString() => $"{this.Uuid} - {this.Utype} - {this.Uname} - {this.TimeStamp}";

        public bool Equals(HeartBeatEntry other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(this.Uuid, other.Uuid) && string.Equals(this.Utype, other.Utype) && string.Equals(this.Uname, other.Uname) && this.TimeStamp.Equals(other.TimeStamp);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return this.Equals((HeartBeatEntry)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (this.Uuid != null ? this.Uuid.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.Utype != null ? this.Utype.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.Uname != null ? this.Uname.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ this.TimeStamp.GetHashCode();
                return hashCode;
            }
        }
    }
}