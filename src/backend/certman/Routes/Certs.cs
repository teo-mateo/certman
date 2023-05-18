// using System.Diagnostics.CodeAnalysis;
// using certman.Models;
// using certman.Services;
// using Dapper;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Data.Sqlite;
//
// namespace certman.Routes;
//
// public static partial class Certs
// {
//     public static readonly Delegate PruneCACerts = async (
//         [FromServices] IConfiguration config) =>
//     {
//         var certs = await GetAllCACertsInternal(config);
//         foreach (var cert in certs)
//         {
//             await PruneCACert(config, cert);
//         }
//     };
//
//     public static readonly Delegate GetAllCACerts = async (
//         [FromServices] IConfiguration config) => await GetAllCACertsInternal(config);
//
//     
//
//
// }