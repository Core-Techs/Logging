// ReaderWriterLockMgr.cs: Manager for a ReaderWriterLockSlim
//
// Copyright 2008-2010 NobleTech Limited.
// This code is the IPR of NobleTech Limited and is freely licenced for use on
//  any project, commercial or otherwise, so long as this copyright notice
//  remains intact. If you improve this code you are asked to submit your
//  modifications for the benefit of the wider community at
//  http://www.NobleTech.co.uk/Articles/ReaderWriterLockMgr.aspx
// Phone +44 844 8466 340 for further licencing options.
//
// This file created by Nathan Phillips of NobleTech Consulting
/////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

// ReSharper disable CheckNamespace
namespace NobleTech.Products.Library
// ReSharper restore CheckNamespace
{
    /// <summary>
    /// Manager for a lock object that acquires and releases the lock in a manner
    ///  that avoids the common problem of deadlock within the using block
    ///  initialisation.
    /// </summary>
    /// <remarks>
    /// This manager object is, by design, not itself thread-safe.
    /// Ronnie Overby - changed class access to internal to avoid conflicts in consuming applications.
    /// </remarks>    
    internal sealed class ReaderWriterLockMgr : IDisposable
    {
        /// <summary>
        /// Local reference to the lock object managed
        /// </summary>
        private ReaderWriterLockSlim _readerWriterLock;

        private enum LockTypes { None, Read, Write, Upgradeable }
        /// <summary>
        /// The type of lock acquired by this manager
        /// </summary>
        private LockTypes _enteredLockType = LockTypes.None;

        /// <summary>
        /// Manager object construction that does not acquire any lock
        /// </summary>
        /// <param name="readerWriterLock">The lock object to manage</param>
        public ReaderWriterLockMgr(ReaderWriterLockSlim readerWriterLock)
        {
            if (readerWriterLock == null)
                throw new ArgumentNullException("readerWriterLock");
            _readerWriterLock = readerWriterLock;
        }

        /// <summary>
        /// Call EnterReadLock on the managed lock
        /// </summary>
        public void EnterReadLock()
        {
            if (_readerWriterLock == null)
                throw new ObjectDisposedException(GetType().FullName);
            if (_enteredLockType != LockTypes.None)
                throw new InvalidOperationException("Create a new ReaderWriterLockMgr for each state you wish to enter");
            // Allow exceptions by the Enter* call to propogate
            //  and prevent updating of _enteredLockType
            _readerWriterLock.EnterReadLock();
            _enteredLockType = LockTypes.Read;
        }

        /// <summary>
        /// Call EnterWriteLock on the managed lock
        /// </summary>
        public void EnterWriteLock()
        {
            if (_readerWriterLock == null)
                throw new ObjectDisposedException(GetType().FullName);
            if (_enteredLockType != LockTypes.None)
                throw new InvalidOperationException("Create a new ReaderWriterLockMgr for each state you wish to enter");
            // Allow exceptions by the Enter* call to propogate
            //  and prevent updating of _enteredLockType
            _readerWriterLock.EnterWriteLock();
            _enteredLockType = LockTypes.Write;
        }

        /// <summary>
        /// Call EnterUpgradeableReadLock on the managed lock
        /// </summary>
        public void EnterUpgradeableReadLock()
        {
            if (_readerWriterLock == null)
                throw new ObjectDisposedException(GetType().FullName);
            if (_enteredLockType != LockTypes.None)
                throw new InvalidOperationException("Create a new ReaderWriterLockMgr for each state you wish to enter");
            // Allow exceptions by the Enter* call to propogate
            //  and prevent updating of _enteredLockType
            _readerWriterLock.EnterUpgradeableReadLock();
            _enteredLockType = LockTypes.Upgradeable;
        }

        /// <summary>
        /// Exit the lock, allowing re-entry later on whilst this manager is in scope
        /// </summary>
        /// <returns>Whether the lock was previously held</returns>
        public bool ExitLock()
        {
            switch (_enteredLockType)
            {
                case LockTypes.Read:
                    _readerWriterLock.ExitReadLock();
                    _enteredLockType = LockTypes.None;
                    return true;
                case LockTypes.Write:
                    _readerWriterLock.ExitWriteLock();
                    _enteredLockType = LockTypes.None;
                    return true;
                case LockTypes.Upgradeable:
                    _readerWriterLock.ExitUpgradeableReadLock();
                    _enteredLockType = LockTypes.None;
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Dispose of the lock manager, releasing any lock held
        /// </summary>
        public void Dispose()
        {
            if (_readerWriterLock != null)
            {
                ExitLock();
                // Tidy up managed resources
                // Release reference to the lock so that it gets garbage collected
                //  when there are no more references to it
                _readerWriterLock = null;
                // Call GC.SupressFinalize to take this object off the finalization
                //  queue and prevent finalization code for this object from
                //  executing a second time.
                GC.SuppressFinalize(this);
            }
        }

        ~ReaderWriterLockMgr()
        {
            if (_readerWriterLock != null)
                ExitLock();
            // Leave references to managed resources so that the garbage collector can follow them
        }
    }
}
