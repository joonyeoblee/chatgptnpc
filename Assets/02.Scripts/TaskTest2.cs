using System;
using System.Threading.Tasks;
using NUnit.Framework.Internal;
using UnityEngine;

public class TaskTest2 : MonoBehaviour
{
   private int number = 0;

   private void Start()
   {
      var task1 = new Task(Test);
      var task2 = new Task(Test);
      
      task1.Start();
      task2.Start();
      
      task1.Wait();
      task2.Wait();
      
      Debug.Log(number);
   }

   private void Test()
   {
      for (int i = 0; i < 10000; ++i)
      {
         number += 1;
      }
   }
}
