var apikeysmodel = require('../models/APIKeysModel.js');

exports.checkCardBalance = function(key, res, callback) {
  apikeysmodel.checkCardCredits(key, res, callback);
}

exports.makeAPIKey = function(callback, res) {
  var key = "";
  var possible = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
  for( var i=0; i < 16; i++ ) {
    key += possible.charAt(Math.floor(Math.random() * possible.length));
  }
  apikeysmodel.addNewKey(key, callback, res);
  //still need to check if key exists, highly doubt it tho
  return key;
}

exports.addCredits = function(apikey, credits) {
  apikeysmodel.addCardCredits(apikey, credits);
}

exports.useCredits = function(apikey, credits) {
  apikeysmodel.useCardCredits(apikey, credits);
}