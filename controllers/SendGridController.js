var sendgrid  = require('sendgrid')(process.env.SENDGRID_USERNAME,
  process.env.SENDGRID_PASSWORD);

exports.sendChargeConfirmation = function(email, key, credits, price){
  var message = "Thank you for your recent purchase.\n";
  message += "Your recent order of " + credits + " for $" + price;
  message += " has been added to your API key: ";
  message += key;

  sendgrid.send({
    to: email,
    from: 'order@ampaas.herokuapp.com',
    subject: 'AmpaaS Order Confirmation',
    text: message
  }, function(err, json) {
  if (err) { return console.error(err); }
    console.log(json);
  });

};