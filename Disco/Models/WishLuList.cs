using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;

namespace WishLuWebApp.Models
{
   //================================================================================================//
   public 
   class WishLuList
   {
      //---------------------------------------------------------------------------------------------//
      private IList<WishLu> wishlus = new List<WishLu>();
      //---------------------------------------------------------------------------------------------//
      public
      WishLuList()
      {
      }
      //---------------------------------------------------------------------------------------------//
      public
      IList<WishLu>
      WishLus
      {
         get
         {
            return wishlus;
         }
      }
      //---------------------------------------------------------------------------------------------//
      private
      void
      AddWishLu(WishLu wishLu)
      {
         wishlus.Add(wishLu);
      }
      //---------------------------------------------------------------------------------------------//
      public
      static
      WishLuList
      GetRandom()
      {
         WishLuList list = new WishLuList();
         do
         {
            list.AddWishLu(WishLu.GetRandom());
         } while (Randomizer.GetBoolean(.8));
         return list;
      }
      //---------------------------------------------------------------------------------------------//
   }
   //================================================================================================//
}