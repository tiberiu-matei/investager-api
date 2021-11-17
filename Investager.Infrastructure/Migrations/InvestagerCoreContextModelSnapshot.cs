﻿// <auto-generated />
using System;
using Investager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Investager.Infrastructure.Migrations
{
    [DbContext(typeof(InvestagerCoreContext))]
    partial class InvestagerCoreContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseIdentityByDefaultColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.10");

            modelBuilder.Entity("Investager.Core.Models.Asset", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<string>("Currency")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Exchange")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Industry")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Provider")
                        .HasColumnType("text");

                    b.Property<string>("Symbol")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("Symbol", "Exchange")
                        .IsUnique();

                    b.ToTable("Asset");
                });

            modelBuilder.Entity("Investager.Core.Models.Currency", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ProviderId")
                        .HasColumnType("text");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("Code")
                        .IsUnique();

                    b.ToTable("Currency");
                });

            modelBuilder.Entity("Investager.Core.Models.CurrencyPair", b =>
                {
                    b.Property<int>("FirstCurrencyId")
                        .HasColumnType("integer");

                    b.Property<int>("SecondCurrencyId")
                        .HasColumnType("integer");

                    b.Property<bool>("HasTimeData")
                        .HasColumnType("boolean");

                    b.Property<string>("Provider")
                        .HasColumnType("text");

                    b.HasKey("FirstCurrencyId", "SecondCurrencyId");

                    b.HasIndex("SecondCurrencyId");

                    b.ToTable("CurrencyPair");
                });

            modelBuilder.Entity("Investager.Core.Models.RefreshToken", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("EncodedValue")
                        .HasColumnType("text");

                    b.Property<DateTime>("LastUsedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("RefreshToken");
                });

            modelBuilder.Entity("Investager.Core.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<string>("DisplayEmail")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("DisplayName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<byte[]>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("bytea");

                    b.Property<byte[]>("PasswordSalt")
                        .IsRequired()
                        .HasColumnType("bytea");

                    b.Property<int>("Theme")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasDefaultValue(0);

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.ToTable("User");
                });

            modelBuilder.Entity("Investager.Core.Models.Watchlist", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<int>("DisplayOrder")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Watchlist");
                });

            modelBuilder.Entity("Investager.Core.Models.WatchlistAsset", b =>
                {
                    b.Property<int>("WatchlistId")
                        .HasColumnType("integer");

                    b.Property<int>("AssetId")
                        .HasColumnType("integer");

                    b.Property<int>("DisplayOrder")
                        .HasColumnType("integer");

                    b.HasKey("WatchlistId", "AssetId");

                    b.HasIndex("AssetId");

                    b.ToTable("WatchlistAsset");
                });

            modelBuilder.Entity("Investager.Core.Models.WatchlistCurrencyPair", b =>
                {
                    b.Property<int>("WatchlistId")
                        .HasColumnType("integer");

                    b.Property<int>("CurrencyPairId")
                        .HasColumnType("integer");

                    b.Property<int?>("CurrencyPairFirstCurrencyId")
                        .HasColumnType("integer");

                    b.Property<int?>("CurrencyPairSecondCurrencyId")
                        .HasColumnType("integer");

                    b.Property<int>("DisplayOrder")
                        .HasColumnType("integer");

                    b.Property<bool>("IsReversed")
                        .HasColumnType("boolean");

                    b.HasKey("WatchlistId", "CurrencyPairId");

                    b.HasIndex("CurrencyPairFirstCurrencyId", "CurrencyPairSecondCurrencyId");

                    b.ToTable("WatchlistCurrencyPair");
                });

            modelBuilder.Entity("Investager.Core.Models.CurrencyPair", b =>
                {
                    b.HasOne("Investager.Core.Models.Currency", "FirstCurrency")
                        .WithMany()
                        .HasForeignKey("FirstCurrencyId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Investager.Core.Models.Currency", "SecondCurrency")
                        .WithMany()
                        .HasForeignKey("SecondCurrencyId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("FirstCurrency");

                    b.Navigation("SecondCurrency");
                });

            modelBuilder.Entity("Investager.Core.Models.RefreshToken", b =>
                {
                    b.HasOne("Investager.Core.Models.User", "User")
                        .WithMany("RefreshTokens")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Investager.Core.Models.Watchlist", b =>
                {
                    b.HasOne("Investager.Core.Models.User", "User")
                        .WithMany("Watchlists")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Investager.Core.Models.WatchlistAsset", b =>
                {
                    b.HasOne("Investager.Core.Models.Asset", "Asset")
                        .WithMany()
                        .HasForeignKey("AssetId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Investager.Core.Models.Watchlist", "Watchlist")
                        .WithMany("Assets")
                        .HasForeignKey("WatchlistId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Asset");

                    b.Navigation("Watchlist");
                });

            modelBuilder.Entity("Investager.Core.Models.WatchlistCurrencyPair", b =>
                {
                    b.HasOne("Investager.Core.Models.Watchlist", "Watchlist")
                        .WithMany("CurrencyPairs")
                        .HasForeignKey("WatchlistId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Investager.Core.Models.CurrencyPair", "CurrencyPair")
                        .WithMany()
                        .HasForeignKey("CurrencyPairFirstCurrencyId", "CurrencyPairSecondCurrencyId");

                    b.Navigation("CurrencyPair");

                    b.Navigation("Watchlist");
                });

            modelBuilder.Entity("Investager.Core.Models.User", b =>
                {
                    b.Navigation("RefreshTokens");

                    b.Navigation("Watchlists");
                });

            modelBuilder.Entity("Investager.Core.Models.Watchlist", b =>
                {
                    b.Navigation("Assets");

                    b.Navigation("CurrencyPairs");
                });
#pragma warning restore 612, 618
        }
    }
}
