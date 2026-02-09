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
import { sessionContext } from './context/ctx_session.js';
import { inputContext } from './context/ctx_input.js';
import { interactionNpcContext } from './context/ctx_interaction_npc.js';
import { timeContext } from './context/ctx_time.js';

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

app.get( "/statistics/input", asyncMiddleware( async (request,response,next) => {
  /******/
  let responseBody = responseBodyGET ;
  const sql = "SELECT * FROM input ORDER BY session_id ASC" ;
  /******/


  /** Add Context */
  responseBody.context = inputContext ;
  /** End */


  /** Maintain Custody Chain */
  if (response.hasOwnProperty('custody_chain')) {
    responseBody.custody_chain = response.custody_chain ;
  }
  responseBody.custody_chain.push( CUSTODIAN ) ;
  /** End */


  /** Query SQL Server */
  /**
    * TODO: SQL Query
    */
  /** End */
  
  response.json( responseBody ) ;
} ) ) ;

app.get( "/statistics/input/:id", asyncMiddleware( async (request,response,next) => {
  /******/
  let responseBody = responseBodyGET ;
  const sql = "SELECT * FROM input WHERE session_id = ?" ;
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
  

  /** Validate Data */
  /**
    * TODO: ensure that the fields contain valid datetime data
    */
  /** End */


  /** Query SQL Server */
  /**
    * TODO: SQL Query
    */
  /** End */
  
  response.json( responseBody ) ;
} ) ) ;

app.get( "/statistics/interaction/npc", asyncMiddleware( async (request,response,next) => {
  /******/
  let responseBody = responseBodyGET ;
  const sql = "SELECT * FROM interaction_npc ORDER BY session_id ASC" ;
  /******/


  /** Add Context */
  responseBody.context = interactionNpcContext ;
  /** End */


  /** Maintain Custody Chain */
  if (response.hasOwnProperty('custody_chain')) {
    responseBody.custody_chain = response.custody_chain ;
  }
  responseBody.custody_chain.push( CUSTODIAN ) ;
  /** End */


  /** Query SQL Server */
  /**
    * TODO: SQL Query
    */
  /** End */
  
  response.json( responseBody ) ;
} ) ) ;

app.get( "/statistics/interaction/npc/:npc_id", asyncMiddleware( async (request,response,next) => {
  /******/
  let responseBody = responseBodyGET ;
  const sql = "SELECT * FROM interaction_npc WHERE npc_id = ?" ;
  let sql_params = [] ;
  /******/


  /** Add Context */
  responseBody.context = interactionNpcContext ;
  /** End */
  

  /** Maintain Custody Chain */
  if( request.hasOwnProperty('body') && request.body.hasOwnProperty('custody_chain') ) {
    responseBody.custody_chain = request.body.custody_chain ;
  }
  responseBody.custody_chain.push( CUSTODIAN ) ;
  /** End */
  

  /** Validate Data */
  /**
    * TODO: ensure that the fields contain valid datetime data
    */
  /** End */


  /** Query SQL Server */
  /**
    * TODO: SQL Query
    */
  /** End */
  
  response.json( responseBody ) ;
} ) ) ;

app.get( "/statistics/interaction/npc/:npc_id/:id", asyncMiddleware( async (request,response,next) => {
  /******/
  let responseBody = responseBodyGET ;
  const sql = "SELECT * FROM interaction_npc WHERE npc_id = ? AND session_id = ?" ;
  let sql_params = [] ;
  /******/


  /** Add Context */
  responseBody.context = interactionNpcContext ;
  /** End */
  

  /** Maintain Custody Chain */
  if( request.hasOwnProperty('body') && request.body.hasOwnProperty('custody_chain') ) {
    responseBody.custody_chain = request.body.custody_chain ;
  }
  responseBody.custody_chain.push( CUSTODIAN ) ;
  /** End */
  

  /** Validate Data */
  /**
    * TODO: ensure that the fields contain valid datetime data
    */
  /** End */


  /** Query SQL Server */
  /**
    * TODO: SQL Query
    */
  /** End */
  
  response.json( responseBody ) ;
} ) ) ;

app.get( "/statistics/time", asyncMiddleware( async (request,response,next) => {
  /******/
  let responseBody = responseBodyGET ;
  const sql = "SELECT * FROM time ORDER BY session_id ASC" ;
  /******/


  /** Add Context */
  responseBody.context = timeContext ;
  /** End */


  /** Maintain Custody Chain */
  if (response.hasOwnProperty('custody_chain')) {
    responseBody.custody_chain = response.custody_chain ;
  }
  responseBody.custody_chain.push( CUSTODIAN ) ;
  /** End */


  /** Query SQL Server */
  /**
    * TODO: SQL Query
    */
  /** End */
  
  response.json( responseBody ) ;
} ) ) ;

app.get( "/statistics/time/:id", asyncMiddleware( async (request,response,next) => {
  /******/
  let responseBody = responseBodyGET ;
  const sql = "SELECT * FROM time WHERE session_id = ?" ;
  let sql_params = [] ;
  /******/


  /** Add Context */
  responseBody.context = timeContext ;
  /** End */
  

  /** Maintain Custody Chain */
  if( request.hasOwnProperty('body') && request.body.hasOwnProperty('custody_chain') ) {
    responseBody.custody_chain = request.body.custody_chain ;
  }
  responseBody.custody_chain.push( CUSTODIAN ) ;
  /** End */
  

  /** Validate Data */
  /**
    * TODO: ensure that the fields contain valid datetime data
    */
  /** End */


  /** Query SQL Server */
  /**
    * TODO: SQL Query
    */
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
    responseBody.custody_chain = request.body.custody_chain ;
  }
  responseBody.custody_chain.push( CUSTODIAN ) ;
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

app.post( "/statistics/input", asyncMiddleware( async (request,response,next) => {
  /******/
  let responseBody = responseBodyPOST ;
  const sql = "INSERT INTO input (session_id,times_buttons_clicked,distance_moved,etc) VALUES (?,?,?,?)" ;
  let sql_params = [] ;
  /******/


  /** Add Context */
  responseBody.context = inputContext ;
  /** End */
  

  /** Maintain Custody Chain */
  if( request.hasOwnProperty('body') && request.body.hasOwnProperty('custody_chain') ) {
    responseBody.custody_chain = request.body.custody_chain ;
  }
  responseBody.custody_chain.push( CUSTODIAN ) ;
  /** End */
  

  /** Validate Data */
  /**
    * TODO: ensure that the fields contain valid datetime data
    */
  /** End */


  /** Query SQL Server */
  /**
    * TODO: SQL Query
    */
  /** End */
  
  response.json( responseBody ) ;
} ) ) ;

app.post( "/statistics/interaction/npc", asyncMiddleware( async (request,response,next) => {
  /******/
  let responseBody = responseBodyPOST ;
  const sql = "INSERT INTO interaction_npc (session_id,npc_id,times_talked_to,lines_of_dialogue_skipped) VALUES (?,?,?,?)" ;
  let sql_params = [] ;
  /******/


  /** Add Context */
  responseBody.context = interactionNpcContext ;
  /** End */
  

  /** Maintain Custody Chain */
  if( request.hasOwnProperty('body') && request.body.hasOwnProperty('custody_chain') ) {
    responseBody.custody_chain = request.body.custody_chain ;
  }
  responseBody.custody_chain.push( CUSTODIAN ) ;
  /** End */
  

  /** Validate Data */
  /**
    * TODO: ensure that the fields contain valid datetime data
    */
  /** End */


  /** Query SQL Server */
  /**
    * TODO: SQL Query
    */
  /** End */
  
  response.json( responseBody ) ;
} ) ) ;

app.post( "/statistics/time", asyncMiddleware( async (request,response,next) => {
  /******/
  let responseBody = responseBodyPOST ;
  const sql = "INSERT INTO time (session_id,in_menus,in_exploration,in_dialogue) VALUES (?,?,?,?)" ;
  let sql_params = [] ;
  /******/


  /** Add Context */
  responseBody.context = timeContext ;
  /** End */
  

  /** Maintain Custody Chain */
  if( request.hasOwnProperty('body') && request.body.hasOwnProperty('custody_chain') ) {
    responseBody.custody_chain = request.body.custody_chain ;
  }
  responseBody.custody_chain.push( CUSTODIAN ) ;
  /** End */
  

  /** Validate Data */
  /**
    * TODO: ensure that the fields contain valid datetime data
    */
  /** End */


  /** Query SQL Server */
  /**
    * TODO: SQL Query
    */
  /** End */
  
  response.json( responseBody ) ;
} ) ) ;


/*************************/
/******* PUT/PATCH *******/
/*************************/

import { responseBodyPUT } from './format/putf.js';

app.put( "/statistics/session/:id", asyncMiddleware( async (request,response,next) => {
  /******/
  let responseBody = responseBodyPUT ;
  const sql = "UPDATE session SET ? = ? WHERE id = ?" ;
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
  

  /** Validate Parameters */
  if( request.hasOwnProperty('params') && request.params.hasOwnProperty('id') && Number.isInteger( parseInt(request.params.id) ) ) {
    responseBody.insert_id = request.params.id ;
  } else {
    responseBody.ok = false ;
    responseBody.error = `Invalid parameter.` ;
    response.status(400) ;
    response.json( responseBody ) ;
    return ;
  }
  /** End*/
  

  /** Validate Data */
  /**
    * TODO: ensure that the fields contain valid datetime data
    */
  /** End */


  /** Query SQL Server */
  /**
    * TODO: SQL Query
    */
  /** End */
  
  response.json( responseBody ) ;
} ) ) ;

app.put( "/statistics/input/:id", asyncMiddleware( async (request,response,next) => {
  /******/
  let responseBody = responseBodyPUT ;
  const sql = "UPDATE input SET ? = ? WHERE id = :id" ;
  let sql_params = [] ;
  /******/


  /** Add Context */
  responseBody.context = inputContext ;
  /** End */
  

  /** Maintain Custody Chain */
  if( request.hasOwnProperty('body') && request.body.hasOwnProperty('custody_chain') ) {
    responseBody.custody_chain = request.body.custody_chain ;
  }
  responseBody.custody_chain.push( CUSTODIAN ) ;
  /** End */
  

  /** Validate Data */
  /**
    * TODO: ensure that the fields contain valid datetime data
    */
  /** End */


  /** Query SQL Server */
  /**
    * TODO: SQL Query
    */
  /** End */
  
  response.json( responseBody ) ;
} ) ) ;

app.put( "/statistics/interaction/npc/:id", asyncMiddleware( async (request,response,next) => {
  /******/
  let responseBody = responseBodyPUT ;
  const sql = "UPDATE interaction_npc SET ? = ? WHERE id = :id" ;
  let sql_params = [] ;
  /******/


  /** Add Context */
  responseBody.context = interactionNpcContext ;
  /** End */
  

  /** Maintain Custody Chain */
  if( request.hasOwnProperty('body') && request.body.hasOwnProperty('custody_chain') ) {
    responseBody.custody_chain = request.body.custody_chain ;
  }
  responseBody.custody_chain.push( CUSTODIAN ) ;
  /** End */
  

  /** Validate Data */
  /**
    * TODO: ensure that the fields contain valid datetime data
    */
  /** End */


  /** Query SQL Server */
  /**
    * TODO: SQL Query
    */
  /** End */
  
  response.json( responseBody ) ;
} ) ) ;

app.put( "/statistics/time/:id", asyncMiddleware( async (request,response,next) => {
  /******/
  let responseBody = responseBodyPUT ;
  const sql = "UPDATE time SET ? = ? WHERE id = :id" ;
  let sql_params = [] ;
  /******/


  /** Add Context */
  responseBody.context = timeContext ;
  /** End */
  

  /** Maintain Custody Chain */
  if( request.hasOwnProperty('body') && request.body.hasOwnProperty('custody_chain') ) {
    responseBody.custody_chain = request.body.custody_chain ;
  }
  responseBody.custody_chain.push( CUSTODIAN ) ;
  /** End */
  

  /** Validate Data */
  /**
    * TODO: ensure that the fields contain valid datetime data
    */
  /** End */


  /** Query SQL Server */
  /**
    * TODO: SQL Query
    */
  /** End */
  
  response.json( responseBody ) ;
} ) ) ;


/*************************/
/******** DELETE *********/
/*************************/

import { responseBodyDELETE } from './format/deletef.js';

app.delete( "/statistics/session/:id", asyncMiddleware( async (request,response,next) => {
  /******/
  let responseBody = responseBodyDELETE ;
  const sql = "DELETE FROM session WHERE id = ?" ;
  let sql_params = [] ;
  responseBody.deletions = [
    { "location": "/statistics/input", "count": 0 },
    { "location": "/statistics/interaction/npc", "count": 0 },
    { "location": "/statistics/time", "count": 0 }
  ] ;
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
  let [ okPacket, _ ] = await sql_connection.execute( sql, sql_params ) ;
  responseBody.deletions.push( { "location": "/statistics/session", "count": okPacket.affectedRows } ) ;
  /** End */
  
  response.json( responseBody ) ;
} ) ) ;

app.delete( "/statistics/input/:id", asyncMiddleware( async (request,response,next) => {
  /******/
  let responseBody = responseBodyDELETE ;
  const sql = "DELETE FROM input WHERE id = :id" ;
  let sql_params = [] ;
  let deletions = {
    "/statistics/input": 0,
    "_comment": "location and number of deletions"
  } ;
  /******/


  /** Add Context */
  responseBody.context = inputContext ;
  /** End */
  

  /** Maintain Custody Chain */
  if( request.hasOwnProperty('body') && request.body.hasOwnProperty('custody_chain') ) {
    responseBody.custody_chain = request.body.custody_chain ;
  }
  responseBody.custody_chain.push( CUSTODIAN ) ;
  /** End */


  /** Query SQL Server */
  /**
    * TODO: SQL Query
    */
  /** End */
  
  response.json( responseBody ) ;
} ) ) ;

app.delete( "/statistics/interaction/npc/:id", asyncMiddleware( async (request,response,next) => {
  /******/
  let responseBody = responseBodyDELETE ;
  const sql = "DELETE FROM interaction_npc WHERE id = :id" ;
  let sql_params = [] ;
  let deletions = {
    "/statistics/interaction/npc": 0,
    "_comment": "location and number of deletions"
  } ;
  /******/


  /** Add Context */
  responseBody.context = interactionNpcContext ;
  /** End */
  

  /** Maintain Custody Chain */
  if( request.hasOwnProperty('body') && request.body.hasOwnProperty('custody_chain') ) {
    responseBody.custody_chain = request.body.custody_chain ;
  }
  responseBody.custody_chain.push( CUSTODIAN ) ;
  /** End */


  /** Query SQL Server */
  /**
    * TODO: SQL Query
    */
  /** End */
  
  response.json( responseBody ) ;
} ) ) ;

app.delete( "/statistics/time/:id", asyncMiddleware( async (request,response,next) => {
  /******/
  let responseBody = responseBodyDELETE ;
  const sql = "DELETE FROM time WHERE id = :id" ;
  let sql_params = [] ;
  let deletions = {
    "/statistics/time": 0,
    "_comment": "location and number of deletions"
  } ;
  /******/


  /** Add Context */
  responseBody.context = timeContext ;
  /** End */
  

  /** Maintain Custody Chain */
  if( request.hasOwnProperty('body') && request.body.hasOwnProperty('custody_chain') ) {
    responseBody.custody_chain = request.body.custody_chain ;
  }
  responseBody.custody_chain.push( CUSTODIAN ) ;
  /** End */


  /** Query SQL Server */
  /**
    * TODO: SQL Query
    */
  /** End */
  
  response.json( responseBody ) ;
} ) ) ;



/*************************/
/********* START *********/
/*************************/

server.listen(LOCALPORT,LOCALHOST);

console.log(`Server listening on: http://${LOCALHOST}:${LOCALPORT}`);