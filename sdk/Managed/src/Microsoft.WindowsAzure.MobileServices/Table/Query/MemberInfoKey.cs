// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    ///  Encapsulates information about a class member that can be used as a key
    ///  in a dictionary without performing a reflection call to instantiate an 
    ///  actual MemberInfo instance.
    /// </summary>
    internal class MemberInfoKey : IEquatable<MemberInfoKey>
    {
        private static Type[] emptyTypeParameters = new Type[0];

        // Information about the class member
        private MemberInfo memberInfo;
        private Type type;
        private String memberName;
        private bool isMethod;
        private bool isInstance;
        private Type[] parameters;

        /// <summary>
        /// Instantiates an instance of a <see cref="MemberInfoKey"/> from a
        /// <see cref="MemberInfo"/>.
        /// </summary>
        /// <param name="memberInfo">
        /// The <see cref="MemberInfo"/> instance that provides information
        /// about the class member.
        /// </param>
        public MemberInfoKey(MemberInfo memberInfo)
        {
            this.memberInfo = memberInfo;
            this.memberName = memberInfo.Name;
            this.type = memberInfo.ReflectedType;

            MethodInfo asMethod = memberInfo as MethodInfo;
            if (asMethod != null)
            {
                this.isMethod = true;
                
                this.isInstance = !asMethod.IsStatic;
                this.parameters = asMethod.GetParameters().Select(p => p.ParameterType).ToArray();
            }
            else
            {
                PropertyInfo asProperty = memberInfo as PropertyInfo;
                Debug.Assert(asProperty != null, "All MemberInfoKey instances must be either methods or properties.");

                this.isMethod = false;
                this.isInstance = true;
                this.parameters = emptyTypeParameters;
            }
        }

        /// <summary>
        /// Instantiates an instance of a <see cref="MemberInfoKey"/> from information
        /// about a class member wihtout having to create a <see cref="MemberInfo"/> instance.
        /// </summary>
        /// <param name="type">
        /// The type of the class that contains the member.
        /// </param>
        /// <param name="memberName">
        /// The name of the class member.
        /// </param>
        /// <param name="isMethod">
        /// <code>true</code> if the member is a method and <code>false</code> 
        /// if the member is a property.
        /// </param>
        /// <param name="isInstance">
        /// <code>true</code> is the member is an instance member and <code>false</code> 
        /// if the member is a class member.
        /// </param>
        /// <param name="parameters">
        /// An array of types for the parameters of the member if the member is a method. 
        /// Should be an empty array if the member is a property.
        /// </param>
        public MemberInfoKey(Type type, string memberName, bool isMethod, bool isInstance, params Type[] parameters)
        {
            this.type = type;
            this.memberName = memberName;
            this.isMethod = isMethod;
            this.isInstance = isInstance;
            this.parameters = parameters;
        }

        /// <summary>
        /// Returns <code>true</code> if the other <see cref="MemberInfoKey"/> instance
        /// represents the same class member as this instance.
        /// </summary>
        /// <param name="other">
        /// The other <see cref="MemberInfoKey"/> to check equivalance against.
        /// </param>
        /// <returns>
        /// <code>true</code> if the other <see cref="MemberInfoKey"/> instance
        /// represents the same class member as this instance.
        /// </returns>
        public bool Equals(MemberInfoKey other)
        {
            bool areEqual = false;

            // If both instances refer to an actual MemberInfo instance, just
            // check those for equivalence.
            if (other.memberInfo != null &&
                this.memberInfo != null)
            {
                areEqual = MemberInfo.Equals(other.memberInfo, this.memberInfo);
            }
            else if (string.Equals(other.memberName, this.memberName, StringComparison.Ordinal) &&
                other.type == this.type &&
                other.isMethod == this.isMethod &&
                other.isInstance == this.isInstance &&
                this.parameters.SequenceEqual(other.parameters))
            {
                areEqual = true;

                // If one of the instances has a MemberInfo instance, set it on the
                // other instance to speed up future equivalence checks.
                if (other.memberInfo == null)
                {
                    other.memberInfo = this.memberInfo;
                }
                else
                {
                    this.memberInfo = other.memberInfo;
                }
            }

            return areEqual;
        }

        /// <summary>
        /// Returns <code>true</code> if the object is a <see cref="MemberInfoKey"/> instance
        /// that represents the same class member as this instance.
        /// </summary>
        /// <param name="obj">
        /// The object to check equivalance against.
        /// </param>
        /// <returns>
        /// <code>true</code> if the object is a <see cref="MemberInfoKey"/> instance
        /// that represents the same class member as the instance.
        /// </returns>
        public override bool Equals(object obj)
        {
            MemberInfoKey other = obj as MemberInfoKey;
            if (other != null)
            {
                return this.Equals(other);
            }

            return false;
        }

        /// <summary>
        /// Returns a hashcode for the instance that is based on the
        /// type and name of the class member.
        /// </summary>
        /// <returns>
        /// A hashcode for the instance that is based on the
        /// type and name of the class member.
        /// </returns>
        public override int GetHashCode()
        {
            return this.memberName.GetHashCode() | this.type.GetHashCode();
        }
    }
}
