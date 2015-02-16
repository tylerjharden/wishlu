using System;
using System.Collections.Generic;
using System.Drawing;

namespace Disco.Models
{
   //================================================================================================//
   public 
   class WishLu
   {
      //---------------------------------------------------------------------------------------------//
      private IList<Wish> wishes = new List<Wish>();
      //---------------------------------------------------------------------------------------------//
      public
      WishLu()
      {
      }
      //---------------------------------------------------------------------------------------------//
      public
      IList<Wish>
      Wishes
      {
         get
         {
            return wishes;
         }
      }
      //---------------------------------------------------------------------------------------------//
      public
      void
      AddWish(Wish wish)
      {
         wishes.Add(wish);
      }
      //---------------------------------------------------------------------------------------------//
      public
      int
      UserId
      { get; set; }
      //---------------------------------------------------------------------------------------------//
      public
      Color
      Color
      { get; set; }
      //---------------------------------------------------------------------------------------------//
      public 
      string 
      Name 
      { get; set; }
      //---------------------------------------------------------------------------------------------//
      public
      DateTime
      EventDate
      { get; set; }
      //---------------------------------------------------------------------------------------------//
      public
      int
      NumberOfWishes
      {
         get
         {
            return wishes.Count;
         }
      }
      //---------------------------------------------------------------------------------------------//
      public 
      bool 
      IsFloating
      { get; set; }
      //---------------------------------------------------------------------------------------------//
      public 
      bool 
      IsBirthday
      { get; set; }
      //---------------------------------------------------------------------------------------------//
      public
      bool
      IsDeletable
      {
         get
         {
            return (!IsFloating && !IsBirthday);
         }
      }
      //---------------------------------------------------------------------------------------------//
      public
      IList<Wish>
      GetMostImportantWishes(int number)
      {
         int getAmount = System.Math.Min(number, wishes.Count);
         IList<Wish> mostImportant = new List<Wish>();
         for (int i = 0; i < getAmount; ++i)
            mostImportant.Add(wishes[i]);
         return mostImportant;
      }
      //---------------------------------------------------------------------------------------------//
      public
      static
      WishLu
      GetRandom()
      {
         WishLu wishLu = new WishLu();
         wishLu.Name = Randomizer.GetAlphaString(5, 30);
         wishLu.EventDate = Randomizer.GetDate(false, DateTime.Now, DateTime.Now.AddYears(1)).Value;
         wishLu.Color = Randomizer.GetColor();
         wishLu.IsBirthday = Randomizer.GetBoolean(.1);
         if (!wishLu.IsBirthday)
            wishLu.IsFloating = Randomizer.GetBoolean(.1);
         do
         {
            wishLu.AddWish(Wish.GetRandom());
         } while (Randomizer.GetBoolean(.6));
         return wishLu;
      }
      //---------------------------------------------------------------------------------------------//
   }
   //================================================================================================//
}