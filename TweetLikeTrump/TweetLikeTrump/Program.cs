/*
 * Date: June 2017
 * Purpose: Implementation of Markov model to tweet like Trump
 * Author: gashort@microsoft.com
 * Copyright (C) 2017 Gary Short
 * License: see license.txt
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace TweetLikeTrump
{
    class Program
    {
        /// <summary>
        /// Program entry point
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Dictionary<string, List<string>> model = CreateModel();
            for (int i = 0; i < 5; i++) { TweetLikeTrump(model); }
        }

        /// <summary>
        /// Using the provided model, generate tweets like Donal Trump
        /// </summary>
        /// <param name="model">The Markov model</param>
        private static void TweetLikeTrump(Dictionary<string, List<string>> model)
        {
            // Start at a random place in the model
            string tweet = String.Empty;
            tweet += RandomString(model);

            // Using the last two words of the last sentence, generate the next sentence
            // If there is no key in the model matching the last two words, begin again
            // from a random place in the model
            while (true)
            {
                if (tweet.Length >= 2)
                {
                    // Get the last two words of the sentence
                    string[] words = tweet.Split();
                    string last2Words = String.Join(
                        " ",
                        words.ToList().GetRange(words.Length - 2, 2).ToArray());
                    last2Words = new string(
                        last2Words
                            .ToCharArray()
                            .Where(c => !char.IsPunctuation(c)).ToArray());

                    // Does the model contain this key?
                    if (model.Keys.Contains(last2Words))
                    {
                        // It does, so probalistically index into the model and fetch the next sentence
                        List<string> values = model[last2Words];
                        int valueKey = new Random(DateTime.Now.Millisecond).Next(values.Count - 1);
                        string value = values.ToArray()[valueKey];
                        tweet += " " + value;

                        // If the lenght of the tweet is longer than 140 characters then we're done
                        if (tweet.ToCharArray().Length > 140) { break; }
                    }
                    // No, the model doesn't contain the key so continue from a random position
                    else { tweet += " " + RandomString(model); }
                }
                else
                {
                    tweet += " " + RandomString(model);
                }
            }
            tweet = new String(tweet
                    .ToCharArray()
                    .Where(c => !char.IsPunctuation(c)).ToArray());
            Console.WriteLine(tweet + "\n\n");
        }

        /// <summary>
        /// Pick a key at random and probalistically reach into the associated sentences
        /// </summary>
        /// <param name="model">The Markov model</param>
        /// <returns></returns>
        private static string RandomString(Dictionary<string, List<string>> model)
        {
            Thread.Sleep(20); // Pause to prevent random seed collisions
            int keyIndex = new Random().Next(model.Keys.Count - 1);
            string key = model.Keys.ToArray()[keyIndex];
            List<string> values = model[key];
            int valueKey = new Random().Next(values.Count - 1);
            string value = values.ToArray()[valueKey];
            return key + " " + value;
        }

        /// <summary>
        /// Create the Markov model based on a provided corpus of text
        /// </summary>
        /// <returns>A dictionary representation of the Markov model</returns>
        private static Dictionary<string, List<string>> CreateModel()
        {
            Dictionary<string, List<string>> model = new Dictionary<string, List<string>>();
            string[] sentences = GetSentences();
            foreach (string sentence in sentences)
            {
                List<string> words = sentence.Split().ToList();
                // Loop through the words creating a moving two word window as the key
                // and the rest of the sentence as the value.
                for (int i = 0; i < words.Count - 1; i++)
                {
                    string[] keyArray = words.GetRange(i, 2).ToArray();
                    string key = String.Join(" ", keyArray).ToUpper();
                    int offset = i + 2;
                    int length = words.Count - (i + 2);
                    string[] valueArray = words.GetRange(i + 2, words.Count - (i + 2)).ToArray();
                    string value = String.Join(" ", valueArray).ToUpper();
                    if (!model.ContainsKey(key))
                    {
                        List<string> values = new List<string>();
                        values.Add(value);
                        model.Add(key, values);
                    }
                    else
                    {
                        model[key].Add(value);
                    }
                }
            }
            return model;
        }

        /// <summary>
        /// A naive algorithm to break the corpus into sentences
        /// </summary>
        /// <returns>An array of sentences</returns>
        private static string[] GetSentences()
        {
            string corpus = File.ReadAllText("TrumpSpeeches.txt");
            return Regex.Split(corpus, @"(?<=[\.!\?])\s+");
        }
    }
}
