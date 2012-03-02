﻿using System;
using LibGit2Sharp.Core;
using LibGit2Sharp.Core.Compat;

namespace LibGit2Sharp
{
    /// <summary>
    ///   A base class for things that wrap a <see cref = "Reference" /> (branch, tag, etc).
    /// </summary>
    /// <typeparam name="TObject">The type of the referenced Git object.</typeparam>
    public abstract class ReferenceWrapper<TObject> where TObject : GitObject
    {
        /// <summary>
        ///   The repo.
        /// </summary>
        protected readonly Repository repo;
        private readonly Lazy<TObject> objectBuilder;

        /// <summary>
        ///   Initializes a new instance of the <see cref = "ReferenceWrapper{TObject}" /> class.
        /// </summary>
        /// <param name="repo">The repo.</param>
        /// <param name="reference">The reference.</param>
        /// <param name="canonicalNameSelector">A function to construct the reference's canonical name.</param>
        protected internal ReferenceWrapper(Repository repo, Reference reference, Func<Reference, string> canonicalNameSelector)
        {
            Ensure.ArgumentNotNull(repo, "repo");
            Ensure.ArgumentNotNull(canonicalNameSelector, "canonicalNameSelector");

            this.repo = repo;
            CanonicalName = canonicalNameSelector(reference);
            objectBuilder = new Lazy<TObject>(() => RetrieveTargetObject(reference));
        }

        /// <summary>
        ///   Gets the full name of this reference.
        /// </summary>
        public string CanonicalName { get; protected set; }

        /// <summary>
        ///   Gets the name of this reference.
        /// </summary>
        public string Name
        {
            get { return Shorten(CanonicalName); }
        }

        /// <summary>
        ///   Returns the <see cref = "CanonicalName" />, a <see cref = "string" /> representation of the current reference.
        /// </summary>
        /// <returns>The <see cref = "CanonicalName" /> that represents the current reference.</returns>
        public override string ToString()
        {
            return CanonicalName;
        }

        /// <summary>
        ///   Gets the <typeparam name = "TObject" /> this <see cref = "ReferenceWrapper{TObject}" /> points to.
        /// </summary>
        protected TObject TargetObject
        {
            get { return objectBuilder.Value; }
        }

        /// <summary>
        ///   Returns the friendly shortened name from a canonical name.
        /// </summary>
        /// <param name="canonicalName">The canonical name to shorten.</param>
        /// <returns></returns>
        protected abstract string Shorten(string canonicalName);

        private TObject RetrieveTargetObject(Reference reference)
        {
            Ensure.ArgumentNotNull(reference, "reference");

            var directReference = reference.ResolveToDirectReference();
            if (directReference == null)
            {
                return null;
            }

            var target = directReference.Target;
            if (target == null)
            {
                return null;
            }

            return repo.Lookup<TObject>(target.Id);
        }
    }
}