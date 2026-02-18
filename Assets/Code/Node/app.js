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
/******* RESPONSES *******/
/*************************/

import { responseBodyGET } from './format/getf.js';
import { responseBodyPOST } from './format/postf.js';
import { responseBodyPUT } from './format/putf.js';
import { responseBodyDELETE } from './format/deletef.js';


/*************************/
/******* FUNCTIONS *******/
/*************************/

function ValidateIntegerParameter(request,paramName) {
  return request.hasOwnProperty('params') && request.params.hasOwnProperty(paramName) && Number.isInteger( parseInt(request.params[paramName]) ) ;
}


/*************************/
/******** SESSION ********/
/*************************/


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

  if( rows.length == 0 )
  {
    responseBody.ok = false ;
    responseBody.error = "No matching data found.";
    response.json( responseBody ) ;
    return ;
  }

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
  responseBody.data = [ {
    "id":           rows[0][fields[0].name],
    "started_at":   rows[0][fields[1].name],
    "ended_at":     rows[0][fields[2].name],
    "recorded_at":  rows[0][fields[3].name],
  } ] ;
  /** End */
  
  response.json( responseBody ) ;
} ) ) ;

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
    response.json( responseBody ) ;
    return ;
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

app.put( "/statistics/session/:id", asyncMiddleware( async (request,response,next) => {
  /******/
  let responseBody = responseBodyPUT ;
  const sql = "UPDATE session SET started_at = ?, ended_at = ? WHERE id = ?" ;
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
    response.json( responseBody ) ;
    return ;
  }
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


  /** Query SQL Server */
  let [ okPacket, _ ] = await sql_connection.execute( sql, sql_params ) ;
  /**
    * TODO: error handling
    */
  responseBody.insert_id = okPacket.insertId ;
  /** End */
  
  response.json( responseBody ) ;
} ) ) ;

app.delete( "/statistics/session/:id", asyncMiddleware( async (request,response,next) => {
  /******/
  let responseBody = responseBodyDELETE ;
  const sql = "DELETE FROM session WHERE id = ?" ;
  let sql_params = [] ;
  responseBody.deletions = [] ;
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


/*************************/
/********* INPUT *********/
/*************************/

app.get( "/statistics/session/input", asyncMiddleware( async (request,response,next) => {
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
  responseBody.data = [ ] ;

  rows.forEach( row => {
    responseBody.data.push( {
      "session_id":           row[fields[0].name],
      "times_buttons_clicked":row[fields[1].name],
      "distance_moved":       row[fields[2].name],
      "etc":                  row[fields[3].name],
      "recorded_at":          row[fields[4].name],
    } )
  } ) ;
  /** End */
  
  response.json( responseBody ) ;
} ) ) ;

app.get( "/statistics/session/:session_id/input", asyncMiddleware( async (request,response,next) => {
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
  

  /** Validate Parameter */
  if( request.hasOwnProperty('params') && request.params.hasOwnProperty('session_id') && Number.isInteger( parseInt(request.params.session_id) ) ) {
    sql_params.push( request.params.session_id ) ;
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
  
  responseBody.data = [ {
    "session_id":           rows[0][fields[0].name],
    "times_buttons_clicked":rows[0][fields[1].name],
    "distance_moved":       rows[0][fields[2].name],
    "etc":                  rows[0][fields[3].name],
    "recorded_at":          rows[0][fields[4].name],
  } ] ;
  /** End */

  
  response.json( responseBody ) ;
} ) ) ;

app.post( "/statistics/session/:session_id/input", asyncMiddleware( async (request,response,next) => {
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
  

  /** Validate Parameter */
  if( request.hasOwnProperty('params') && request.params.hasOwnProperty('session_id') && Number.isInteger( parseInt(request.params.session_id) ) ) {
    sql_params.push( request.params.session_id ) ;
  } else {
    responseBody.ok = false ;
    responseBody.error = `Invalid parameter.` ;
    response.status(400) ;
    response.json( responseBody ) ;
    return ;
  }
  /** End */
  

  /** Validate Data */
  if( request.hasOwnProperty('body') && request.body.hasOwnProperty('times_buttons_clicked') && request.body.hasOwnProperty('distance_moved') && request.body.hasOwnProperty('etc') ) {
    /**
     * TODO: ensure that the fields contain valid datetime data
     */
    sql_params.push( request.body.times_buttons_clicked ) ;
    sql_params.push( request.body.distance_moved ) ;
    sql_params.push( request.body.etc ) ;
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
  /**
    * TODO: error handling
    */
  responseBody.insert_id = okPacket.insertId ;
  /** End */
  
  response.json( responseBody ) ;
} ) ) ;

app.put( "/statistics/session/:session_id/input", asyncMiddleware( async (request,response,next) => {
  /******/
  let responseBody = responseBodyPUT ;
  const sql = "UPDATE input SET times_buttons_clicked = ?, distance_moved = ?, etc = ? WHERE session_id = ?" ;
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
  if( request.hasOwnProperty('body') && request.body.hasOwnProperty('times_buttons_clicked') && request.body.hasOwnProperty('distance_moved') && request.body.hasOwnProperty('etc') ) {
    /**
     * TODO: ensure that the fields contain valid datetime data
     */
    sql_params.push( request.body.times_buttons_clicked ) ;
    sql_params.push( request.body.distance_moved ) ;
    sql_params.push( request.body.etc ) ;
  } else {
    responseBody.ok = false ;
    responseBody.error = `Invalid parameter.` ;
    response.status(400) ;
    response.json( responseBody ) ;
    return ;
  }
  /** End */
  

  /** Validate Parameter */
  if( request.hasOwnProperty('params') && request.params.hasOwnProperty('session_id') && Number.isInteger( parseInt(request.params.session_id) ) ) {
    sql_params.push( request.params.session_id ) ;
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
  /**
    * TODO: error handling
    */
  responseBody.insert_id = okPacket.insertId ;
  /** End */
  
  response.json( responseBody ) ;
} ) ) ;

app.delete( "/statistics/session/:session_id/input", asyncMiddleware( async (request,response,next) => {
  /******/
  let responseBody = responseBodyDELETE ;
  const sql = "DELETE FROM input WHERE id = ?" ;
  let sql_params = [] ;
  responseBody.deletions = [] ;
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
  

  /** Validate Parameter */
  if( request.hasOwnProperty('params') && request.params.hasOwnProperty('session_id') && Number.isInteger( parseInt(request.params.session_id) ) ) {
    sql_params.push( request.params.session_id ) ;
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
  responseBody.deletions.push( { "location": "/statistics/session/input", "count": okPacket.affectedRows } ) ;
  /** End */
  
  response.json( responseBody ) ;
} ) ) ;


/*************************/
/********* TIME **********/
/*************************/

app.get( "/statistics/session/time", asyncMiddleware( async (request,response,next) => {
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
  responseBody.data = [ ] ;

  rows.forEach( row => {
    responseBody.data.push( {
      "session_id":     row[fields[0].name],
      "in_menus":       row[fields[1].name],
      "in_exploration": row[fields[2].name],
      "in_dialogue":    row[fields[3].name],
      "recorded_at":    row[fields[4].name],
    } )
  } ) ;
  /** End */
  

  response.json( responseBody ) ;
} ) ) ;

app.get( "/statistics/session/:session_id/time", asyncMiddleware( async (request,response,next) => {
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
  

  /** Validate Parameter */
  if( ValidateIntegerParameter(request,'session_id') ) {
    sql_params.push( request.params.session_id ) ;
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

  responseBody.data = [ {
    "session_id":     rows[0][fields[0].name],
    "in_menus":       rows[0][fields[1].name],
    "in_exploration": rows[0][fields[2].name],
    "in_dialogue":    rows[0][fields[3].name],
    "recorded_at":    rows[0][fields[4].name],
  } ] ;
  /** End */
  

  response.json( responseBody ) ;
} ) ) ;

app.post( "/statistics/session/:session_id/time", asyncMiddleware( async (request,response,next) => {
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
  

  /** Validate Parameter */
  if( request.hasOwnProperty('params') && request.params.hasOwnProperty('session_id') && Number.isInteger( parseInt(request.params.session_id) ) ) {
    sql_params.push( request.params.session_id ) ;
  } else {
    responseBody.ok = false ;
    responseBody.error = `Invalid parameter.` ;
    response.status(400) ;
    response.json( responseBody ) ;
    return ;
  }
  /** End */
  

  /** Validate Data */
  if( request.hasOwnProperty('body') && request.body.hasOwnProperty('in_menus') && request.body.hasOwnProperty('in_exploration') && request.body.hasOwnProperty('in_dialogue') ) {
    /**
     * TODO: ensure that the fields contain valid datetime data
     */
    sql_params.push( request.body.in_menus ) ;
    sql_params.push( request.body.in_exploration ) ;
    sql_params.push( request.body.in_dialogue ) ;
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
  /**
    * TODO: error handling
    */
  responseBody.insert_id = okPacket.insertId ;
  /** End */
  
  response.json( responseBody ) ;
} ) ) ;

app.put( "/statistics/session/:session_id/time", asyncMiddleware( async (request,response,next) => {
  /******/
  let responseBody = responseBodyPUT ;
  const sql = "UPDATE time SET in_menus = ?, in_exploration = ?, in_dialogue = ? WHERE session_id = ?" ;
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
  if( request.hasOwnProperty('body') && request.body.hasOwnProperty('in_menus') && request.body.hasOwnProperty('in_exploration') && request.body.hasOwnProperty('in_dialogue') ) {
    /**
     * TODO: ensure that the fields contain valid datetime data
     */
    sql_params.push( request.body.in_menus ) ;
    sql_params.push( request.body.in_exploration ) ;
    sql_params.push( request.body.in_dialogue ) ;
  } else {
    responseBody.ok = false ;
    responseBody.error = `Invalid parameter.` ;
    response.status(400) ;
    response.json( responseBody ) ;
    return ;
  }
  /** End */
  

  /** Validate Parameter */
  if( request.hasOwnProperty('params') && request.params.hasOwnProperty('session_id') && Number.isInteger( parseInt(request.params.session_id) ) ) {
    sql_params.push( request.params.session_id ) ;
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
  /**
    * TODO: error handling
    */
  responseBody.insert_id = okPacket.insertId ;
  /** End */
  
  response.json( responseBody ) ;
} ) ) ;

app.delete( "/statistics/session/:session_id/time", asyncMiddleware( async (request,response,next) => {
  /******/
  let responseBody = responseBodyDELETE ;
  const sql = "DELETE FROM time WHERE id = ?" ;
  let sql_params = [] ;
  responseBody.deletions = [] ;
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
  

  /** Validate Parameter */
  if( request.hasOwnProperty('params') && request.params.hasOwnProperty('session_id') && Number.isInteger( parseInt(request.params.session_id) ) ) {
    sql_params.push( request.params.session_id ) ;
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
  responseBody.deletions.push( { "location": "/statistics/session/time", "count": okPacket.affectedRows } ) ;
  /** End */
  
  response.json( responseBody ) ;
} ) ) ;

/*************************/
/********* START *********/
/*************************/

server.listen(LOCALPORT,LOCALHOST);

console.log(`Server listening on: http://${LOCALHOST}:${LOCALPORT}`);