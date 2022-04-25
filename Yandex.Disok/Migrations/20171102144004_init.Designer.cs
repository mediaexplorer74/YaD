using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Ya.D.Services;

namespace Ya.D.Migrations
{
    [DbContext(typeof(LocalContext))]
    [Migration("20171102144004_init")]
    partial class init
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.3");

            modelBuilder.Entity("Ya.D.Models.DiskItem", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<byte[]>("BigPreviewImage");

                    b.Property<int>("Code");

                    b.Property<long>("ContentLength");

                    b.Property<DateTime>("Created");

                    b.Property<string>("Description");

                    b.Property<string>("DisplayName");

                    b.Property<string>("Error");

                    b.Property<bool>("IsFolder");

                    b.Property<string>("ItemType");

                    b.Property<string>("MimeType");

                    b.Property<DateTime>("Modified");

                    b.Property<string>("ParentFolder");

                    b.Property<string>("Path");

                    b.Property<byte[]>("PreviewImage");

                    b.Property<string>("PreviewURL");

                    b.Property<string>("PublicURL");

                    b.Property<string>("PureResponse");

                    b.HasKey("ID");

                    b.ToTable("Items");
                });

            modelBuilder.Entity("Ya.D.Models.ItemList", b =>
                {
                    b.Property<int>("ItemID");

                    b.Property<int>("PlayListID");

                    b.HasKey("ItemID", "PlayListID");

                    b.HasIndex("PlayListID");

                    b.ToTable("ItemsInPlaylist");
                });

            modelBuilder.Entity("Ya.D.Models.PlayList", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.Property<int>("TypeID");

                    b.HasKey("ID");

                    b.HasIndex("TypeID");

                    b.ToTable("PlayLists");
                });

            modelBuilder.Entity("Ya.D.Models.PlayListType", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("ID");

                    b.ToTable("PlayListTypes");
                });

            modelBuilder.Entity("Ya.D.Models.ItemList", b =>
                {
                    b.HasOne("Ya.D.Models.DiskItem", "Item")
                        .WithMany("PlayLists")
                        .HasForeignKey("ItemID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Ya.D.Models.PlayList", "PlayList")
                        .WithMany("Items")
                        .HasForeignKey("PlayListID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Ya.D.Models.PlayList", b =>
                {
                    b.HasOne("Ya.D.Models.PlayListType", "Type")
                        .WithMany()
                        .HasForeignKey("TypeID")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
