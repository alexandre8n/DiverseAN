// !!! Uncomment module (see below)
// if you use the module in node.js
// Uncomment this if you use the module in node.js
module.exports = {
  isPrimeNumber: isPrimeNumber,
  isPrime: isPrime,
  isPrime2: isPrime2,
  getRandomNumber: getRandomNumber,
  getRandomIntFromTo: getRandomIntFromTo,
};

// isPrimeNumber
// Example usage:
// console.log(isPrimeNumber(7)); // Output: true
// console.log(isPrimeNumber(10)); // Output: false
function isPrimeNumber(number) {
  // Check if the number is less than 2, which is not prime
  if (number < 2) {
    return false;
  }
  // Check for divisibility by numbers from 2 to the square root of the number
  for (let i = 2; i <= Math.sqrt(number); i++) {
    // If the number is divisible by any other number, it's not prime
    if (number % i === 0) {
      return false;
    }
  }
  // If no divisors found, it's a prime number
  return true;
}

function isPrime(number) {
  if (number <= 1) return false;

  // The check for the number 2 and 3
  if (number <= 3) return true;

  if (number % 2 == 0 || number % 3 == 0) return false;

  for (var i = 5; i * i <= number; i = i + 6) {
    if (number % i == 0 || number % (i + 2) == 0) return false;
  }
  return true;
}

function isPrime2(num) {
  var sqrtnum = Math.floor(Math.sqrt(num));
  var prime = num != 1;
  for (var i = 2; i < sqrtnum + 1; i++) {
    // sqrtnum+1
    if (num % i == 0) {
      prime = false;
      break;
    }
  }
  return prime;
}

function getRandomNumber(size) {
  return Math.floor(Math.random() * size);
}

// this function returns the number from "from" to "to";
function getRandomIntFromTo(from, to) {
  var size = to - from + 1;
  return from + getRandomNumber(size);
}
