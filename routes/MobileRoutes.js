var apicontroller = require("../controllers/APIController");
var lobcontroller = require("../controllers/LobController");

// gets remaining postcards
// /api/v1/card/balance?key=_APIKEY_
// _APIKEY_ is the key provided to customer upon payment
exports.cardbalance = function(req, res) {
  var key = req.params.key;
  console.log('Retrieving key: ' + key);
  var response = {
    "key": key,
    "postcards remaining": apicontroller.getCardBalance(key)
  };
  response = JSON.stringify(response);
  res.send(response);
}

// send postcards
// /api/v1/card/send?key=_APIKEY
// to_name, to_address1, to_address2, to_city, to_state, to_zip, to_country,
// from_name, from_address1, from_address2, from_city, from_state,
// from_zip, from_country,
// card_design, message,
exports.sendcard = function(req, res) {
  apicontroller.checkCardBalance(req.body.apikey, res, function(){
    var cardinfo = {
      to: {
        name: req.body.to_name,
        address_line1: req.body.to_address1,
        address_line2: req.body.to_address2,
        address_city: req.body.to_city,
        address_state: req.body.to_state,
        address_zip: req.body.to_zip,
        address_country: req.body.to_country
      },
      from: {
        name: req.body.from_name,
        address_line1: req.body.from_address1,
        address_line2: req.body.from_address2,
        address_city: req.body.from_city,
        address_state: req.body.from_state,
        address_zip: req.body.from_zip,
        address_country: req.body.from_country
      },
      message: req.body.message
    };
    lobcontroller.sendPostCard(cardinfo, res, req.body.card_design, function(){
      apicontroller.useCredits(req.body.apikey, 1);
    });
  });
}