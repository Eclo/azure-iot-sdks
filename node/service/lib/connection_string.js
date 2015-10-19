// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

'use strict';

var createDictionary = require('azure-iot-common').createDictionary;
var ArgumentError = require('azure-iot-common').errors.ArgumentError;

function ConnectionString() {}

ConnectionString.parse = function parse(source) {
  /*Codes_SRS_NODE_IOTHUB_CONNSTR_05_001: [The input argument source shall be converted to string if necessary.]*/
  var dict = createDictionary(source, ';');
  var err = 'The connection string is missing the property: ';

  /*Codes_SRS_NODE_IOTHUB_CONNSTR_05_005: [The parse method shall throw ArgumentError if any of 'HostName', 'SharedAccessKeyName', or 'SharedAccessKey' fields are not found in the input argument.]*/
  [
    'HostName',
    'SharedAccessKeyName',
    'SharedAccessKey'
  ].forEach(function (key) {
    if (!(key in dict)) throw new ArgumentError(err + key);
  });

  /*Codes_SRS_NODE_IOTHUB_CONNSTR_05_002: [The parse method shall create a new instance of ConnectionString.]*/
  var cn = new ConnectionString();

  /*Codes_SRS_NODE_IOTHUB_CONNSTR_05_003: [It shall accept a string argument of the form 'name=value[;name=value…]' and for each name extracted it shall create a new property on the ConnectionString object instance.]*/
  /*Codes_SRS_NODE_IOTHUB_CONNSTR_05_004: [The value of the property shall be the value extracted from the input argument for the corresponding name.]*/
  Object.keys(dict).forEach(function (key) {
    cn[key] = dict[key];
  });

  /*Codes_SRS_NODE_IOTHUB_CONNSTR_05_006: [The generated ConnectionString object shall be returned to the caller.]*/
  return cn;
};

module.exports = ConnectionString;