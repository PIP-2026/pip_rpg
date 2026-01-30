#!/usr/bin/node
"use strict" ;
const CUSTODIAN = "ResponseServer";
const LOCALHOST = "127.0.0.1" ; // 192.168.189.59
const LOCALPORT = 4141 ; // 80


/*************************/
/****** BASIC SETUP ******/
/*************************/
import express from 'express';
import http from 'http' ;

var app = express();

var server = http.createServer(app);

app.use( express.json() ) ;


/*************************/
/********* MYSQL *********/
/*************************/

import { createConnection } from 'mysql2/promise';
import { asyncMiddleware } from './util/asyncMiddleware.js';
import { sessionContext } from './context/sessionc.js';

let sql_connection = await createConnection({
  host: "localhost",
  user: "root",
  password: "",
  database: "pip_rpg",
  namedPlaceholders: true,
});

sql_connection.connect(function(err) {
  if (err) throw err;
  console.log("Connected!");
});


/*************************/
/********** GET **********/
/*************************/

import { responseBodyGET } from './format/getf.js';

app.get( "/statistics/session", asyncMiddleware( async (request,response,next) => {
  /******/
  let responseBody = responseBodyGET ;
  const sql = "SELECT * FROM session ORDER BY id ASC" ;
  /******/


  /** Add Context */
  responseBody.context = sessionContext ;
  /** End */


  /** Maintain Custody Chain */
  if (response.hasOwnProperty('custody_chain')) {
    responseBody.custody_chain = response.custody_chain ;
  }
  responseBody.custody_chain.push( CUSTODIAN ) ;
  /** End */


  /** Query SQL Server */
  let [ rows, fields ] = await sql_connection.execute( sql ) ;

  responseBody.data = [ ] ;

  rows.forEach( row => {
    responseBody.data.push( {
      "id":           row[fields[0].name],
      "started_at":   row[fields[1].name],
      "ended_at":     row[fields[2].name],
      "recorded_at":  row[fields[3].name],
    } ) ;
  } ) ;
  /** End */
  
  response.json( responseBody ) ;
} ) ) ;


app.get( "/statistics/session/:id", asyncMiddleware( async (request,response,next) => {
  /******/
  let responseBody = responseBodyGET ;
  const sql = "SELECT * FROM session WHERE id = ?" ;
  let sql_params = [] ;
  /******/


  /** Add Context */
  responseBody.context = sessionContext ;
  /** End */
  

  /** Maintain Custody Chain */
  if( request.hasOwnProperty('body') && request.body.hasOwnProperty('custody_chain') ) {
    responseBody.custody_chain = request.body.custody_chain ;
  }
  responseBody.custody_chain.push( CUSTODIAN ) ;
  /** End */
  

  /** Validate Parameter */
  if( request.hasOwnProperty('params') && request.params.hasOwnProperty('id') && Number.isInteger( parseInt(request.params.id) ) ) {
    sql_params.push( request.params.id ) ;
  } else {
    responseBody.ok = false ;
    responseBody.error = `Invalid parameter.` ;
    response.status(400) ;
    response.json( responseBody ) ;
    return ;
  }
  /** End */


  /** Query SQL Server */
  let [ rows, fields ] = await sql_connection.execute( sql, sql_params ) ;
  /**
    * TODO: error handling
    * ERR: rows contains no elements
    */
  if( rows.length == 0 )
  {
    responseBody.ok = false ;
    responseBody.error = "No matching data found.";
    response.json( responseBody ) ;
    return ;
  }
  responseBody.data = {
    "id":           rows[0][fields[0].name],
    "started_at":   rows[0][fields[1].name],
    "ended_at":     rows[0][fields[2].name],
    "recorded_at":  rows[0][fields[3].name],
  } ;
  /** End */
  
  response.json( responseBody ) ;
} ) ) ;


/*************************/
/********* POST **********/
/*************************/

import { responseBodyPOST } from './format/postf.js';

app.post( "/statistics/session", asyncMiddleware( async (request,response,next) => {
  /******/
  let responseBody = responseBodyPOST ;
  const sql = "INSERT INTO session (started_at,ended_at) VALUES (?,?)" ;
  let sql_params = [] ;
  /******/


  /** Add Context */
  responseBody.context = sessionContext ;
  /** End */
  

  /** Maintain Custody Chain */
  if( request.hasOwnProperty('body') && request.body.hasOwnProperty('custody_chain') ) {
    responseBodyGET.custody_chain = request.body.custody_chain ;
  }
  responseBodyGET.custody_chain.push( CUSTODIAN ) ;
  /** End */
  

  /** Validate Data */
  if( request.hasOwnProperty('body') && request.body.hasOwnProperty('started_at') && request.body.hasOwnProperty('ended_at') ) {
    /**
     * TODO: ensure that the fields contain valid datetime data
     */
    sql_params.push( request.body.started_at ) ;
    sql_params.push( request.body.ended_at ) ;
  } else {
    responseBody.ok = false ;
    responseBody.error = `Invalid parameter.` ;
    response.status(400) ;
  }
  /** End */


  /** Query SQL Server */
  let [ okPacket, _ ] = await sql_connection.execute( sql, sql_params ) ;
  /**
    * TODO: error handling
    */
  responseBody.insert_id = okPacket.insertId ;
  /** End */
  
  response.json( responseBody ) ;
} ) ) ;


/*************************/
/******* PUT/PATCH *******/
/*************************/

import { responseBodyPUT } from './format/putf.js';

app.put( "/statistics/session", asyncMiddleware( async (request,response,next) => {
  /******/
  //let responseBody = responseBodyPUT ;
  //const sql = "UPDATE session SET ? = ? WHERE id = :id" ;
  //let sql_params = [] ;
  /******/


  /** Add Context */
  //responseBody.context = sessionContext ;
  /** End */
  

  /** Maintain Custody Chain */
  //if( request.hasOwnProperty('body') && request.body.hasOwnProperty('custody_chain') ) {
  //  responseBodyGET.custody_chain = request.body.custody_chain ;
  //}
  //responseBodyGET.custody_chain.push( CUSTODIAN ) ;
  /** End */
  

  /** Validate Data */
  //if( request.hasOwnProperty('body') && request.body.hasOwnProperty('started_at') && request.body.hasOwnProperty('ended_at') ) {
    /**
     * TODO: ensure that the fields contain valid datetime data
     */
  //  sql_params.push( request.body.started_at ) ;
  //  sql_params.push( request.body.ended_at ) ;
  //} else {
  //  responseBody.ok = false ;
  //  responseBody.error = `Invalid parameter.` ;
  //  response.status(400) ;
  //}
  /** End */


  /** Query SQL Server */
  //let [ okPacket, _ ] = await sql_connection.execute( sql, sql_params ) ;
  /**
    * TODO: error handling
    */
  //responseBody.insert_id = okPacket.insertId ;
  /** End */
  
  response.json( /* responseBody */ ) ;
} ) ) ;


/*************************/
/******** DELETE *********/
/*************************/

import { responseBodyDELETE } from './format/deletef.js';

app.delete( "/statistics/session", asyncMiddleware( async (request,response,next) => {
  /******/
  let responseBody = responseBodyDELETE ;
  const sql = "DELETE FROM session WHERE id = :id" ;
  let sql_params = [] ;
  let deletions = {
    "/statistics/session": 0,
    "/statistics/input": 0,
    "/statistics/interaction/npc": 0,
    "/statistics/time": 0,
    "_comment": "location and number of deletions"
  } ;
  /******/


  /** Add Context */
  responseBody.context = sessionContext ;
  /** End */
  

  /** Maintain Custody Chain */
  if( request.hasOwnProperty('body') && request.body.hasOwnProperty('custody_chain') ) {
    responseBodyGET.custody_chain = request.body.custody_chain ;
  }
  responseBodyGET.custody_chain.push( CUSTODIAN ) ;
  /** End */
  

  /** Validate Data */
  if( request.hasOwnProperty('body') && request.body.hasOwnProperty('started_at') && request.body.hasOwnProperty('ended_at') ) {
    /**
     * TODO: ensure that the fields contain valid datetime data
     */
    sql_params.push( request.body.started_at ) ;
    sql_params.push( request.body.ended_at ) ;
  } else {
    responseBody.ok = false ;
    responseBody.error = `Invalid parameter.` ;
    response.status(400) ;
  }
  /** End */


  /** Query SQL Server */
  let [ okPacket, _ ] = await sql_connection.execute( sql, sql_params ) ;
  /**
    * TODO: error handling
    */
  responseBody.insert_id = okPacket.insertId ;
  /** End */
  
  response.json( responseBody ) ;
} ) ) ;


/*************************/
/********* START *********/
/*************************/

server.listen(LOCALPORT,LOCALHOST);

console.log(`Server listening on: http://${LOCALHOST}:${LOCALPORT}`);