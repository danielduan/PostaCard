var Mongoose = require('mongoose');

var db = Mongoose.connection;

db.on('error', console.error);
db.once('open', function() {
  // Create your schemas and models here.
});

var mongoUri = process.env.MONGOHQ_URL || 'mongodb://localhost/mydb';
Mongoose.connect(mongoUri);

var APIKeysSchema = new Mongoose.Schema({
  key: {
    type: String,
    required: true
  },
  card_balance: {
    type: Number,
    required: true
  }
});

var APIKey = Mongoose.model('APIKeys', APIKeysSchema);

exports.addNewKey = function(key, callback, res) {
  var apikey = new APIKey({
    key: key,
    card_balance: 0,
  });
  apikey.save(function(err, apikey) {
    if (err) {
      return console.log(err);
    }
    callback(key, res);
    //console.log(apikey);
  })
}

exports.addCardCredits = function(key, credits) {
  APIKey.findOne({key:key}, function (err, apikey) {
    if (err) {
      return console.log(err);
    }
    //console.log(apikey);
    apikey.card_balance += credits;
    apikey.save(function(err, apikey) {
      if (err) {
        return console.log(err);
      }
      //console.log(apikey);
    });
  });
}

exports.getCardCredits = function(key) {
  APIKey.findOne({key:key}, function (err, apikey) {
    if (err) {
      return console.log(err);
    }
    return apikey.card_balance;
  });
}

exports.useCardCredits = function(key, credits) {
  APIKey.findOne({key:key}, function (err, apikey) {
    if (err) {
      return console.log(err);
    }
    //console.log(apikey);
    apikey.card_balance -= credits;
    console.log(apikey.card_balance);
    apikey.save(function(err, apikey) {
      if (err) {
        return console.log(err);
      }
      //console.log(apikey);
    });
  });
}

exports.checkCardCredits = function(key, res, callback) {
  APIKey.findOne({key:key}, function (err, apikey) {
    if (err) {
      res.jsonp(err);
      return console.log(err);
    }
    
    if (apikey.card_balance > 0) {
      callback();
    } else {
      res.jsonp("No more credits remaining. Please purchase more.")
    }
  });  
}


