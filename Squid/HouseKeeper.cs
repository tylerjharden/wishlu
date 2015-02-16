//================================================================================================//
// Housekeeper Class.                                                                             //
//                                                                                                //
// SYNOPSIS                                                                                       //
//      This unit implements the Housekeeper Class.                                               //
//                                                                                                //
// DESCRIPTION                                                                                    //
//      This unit implements the Housekeeper Class.  This class is responsible for performing     //
// periodic housekeeping tasks, like sending queued e-mails.                                      //
//                                                                                                //
// Copyright (c) 2013 WishLu, Inc.  All rights reserved.                                          //
//------------------------------------------------------------------------------------------------//

using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using WishLuWebServiceShared.Config;
using WishLuWebServiceShared.Database;
using WishLuWebServiceShared.Mail;

namespace WishLuWebServiceShared.Housekeeping
{
//================================================================================================//
// Housekeeper Class.                                                                             //
//                                                                                                //
/// <summary>
/// <para>
///      This class is responsible for performing periodic housekeeping tasks, like sending
/// queued e-mails.
/// </para>
/// </summary>
///
public
class Housekeeper
{
   //---------------------------------------------------------------------------------------------//
   // Perform Housekeeping Tasks                                                                  //
   //                                                                                             //
   /// <summary>
   /// <para>
   ///      This procedure performs housekeeping tasks until no tasks remain or until the
   /// specified quit time has been reached.
   /// </para>
   /// </summary>
   ///
   /// <param name="databaseInstance">
   ///      The database instance to be used to perform the tasks.  This may not be NULL.
   /// </param>
   ///
   /// <param name="quitTime">
   ///      The (approximate) time at which procedure should return even if there are tasks
   /// remaing to be performed.  The procedure may return prior to this time, but should not
   /// return significantly after it.
   /// </param>
   ///
   public
   void
   PerformHousekeepingTasks(DatabaseInstance databaseInstance,
                            DateTime         quitTime)
   {
      MailTransmitter mailTransmitter = new MailTransmitter();

      mailTransmitter.ProcessEMails(databaseInstance,quitTime);
   }
   //---------------------------------------------------------------------------------------------//
   // Perform Housekeeping Tasks                                                                  //
   //                                                                                             //
   /// <summary>
   /// <para>
   ///      This procedure performs housekeeping tasks until no tasks remain or until the
   /// specified quit time has been reached.
   /// </para>
   /// </summary>
   ///
   /// <param name="databaseInstance">
   ///      The database instance to be used to perform the tasks.  This may not be NULL.
   /// </param>
   ///
   /// <param name="maxRunSeconds">
   ///      The (approximate) maximum number of seconds for which the procedure should run even
   /// if there are tasks remaing to be performed.  The procedure may return before this time has
   /// expired, but should not return significantly after it has expired.
   /// </param>
   ///
   public
   void
   PerformHousekeepingTasks(DatabaseInstance databaseInstance,
                            int              maxRunSeconds)
   {
      PerformHousekeepingTasks(databaseInstance,DateTime.Now.AddSeconds(maxRunSeconds));
   }
   //---------------------------------------------------------------------------------------------//
}
//================================================================================================//
}

