/////此文件由Source Generator生成
//using Microsoft.EntityFrameworkCore;
//using System.Reflection;
//using System.Text.RegularExpressions;
//using System.ComponentModel.DataAnnotations;
//using Volo.Abp;
//using Volo.Abp.Domain.Entities;
//using Volo.Abp.EntityFrameworkCore.Modeling;
//using System;
//using System.Linq;
//using ExampleDomain.Model;//这里是Model的命名空间

//namespace ExampleForMySourceGenerator//这里是Context所在的命名空间
//{
//    public static partial class ModelAutoCreating
//    {
//        public static void AutoBuild2(ModelBuilder modelBuilder)
//        {

//            modelBuilder.Entity<ExampleModel2>(entity =>
//            {
//                entity.ToTable("example_model2");
//                entity.Property(e => e.Name)
//                .HasColumnType("ntext")
//                .HasColumnName("Name");

//                entity.Property(e => e.ExampleModel1)
//                .HasColumnType("ExampleModel1")
//                .HasColumnName("Examplemodel1");

//                //（测试用）虚拟类有以下这些:
//            });

//            modelBuilder.Entity<ExampleModel1>(entity =>
//            {
//                entity.ToTable("example_model1");
//                entity.Property(e => e.ExampleId)
//                .HasColumnType("int")
//                .HasColumnName("Exampleid");//KeyAttribute

//                entity.Property(e => e.ExampleName)
//                .HasColumnType("ntext")
//                .HasColumnName("Examplename");

//                //（测试用）虚拟类有以下这些:
//            });

//        }
//    }
//}