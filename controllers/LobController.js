//var lobkey = process.env.LOBKEY_TEST;
var LOB = new (require('lob'))(process.env.LOBKEY_LIVE);

exports.sendPostCard = function(postcard, res, design, callback) {
  //var fronturl = "http://ampaas.herokuapp.com/cards/";
  //fronturl += design;
  postcard.front = design;
  LOB.postcards.create(postcard, function(err, response) {
    console.log("sending card");
    if (err) {
      var error = {};
      error.err = err[0].message;
      res.jsonp(error);
      return;
    }
    callback();
    var response = {};
    response.success = "Postcard sent!";
    res.jsonp(response);
  });
}