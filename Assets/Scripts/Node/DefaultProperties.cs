using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class DefaultProperties
{
    public static string ForNode(Node.NodeTypes nodeType)
    {
        string properties = "";
        switch (nodeType)
        {
            case Node.NodeTypes.Person:
                {
                    properties =
                        "knaUid: " + '\n' +
                        "name: " + '\n' +
                        "ssn: " + '\n' +
                        "email: " + '\n' +
                        "phone: " + '\n' +
                        "street: " + '\n' +
                        "postalTown: " + '\n' +
                        "postalCode: ";
                    break;
                }
            case Node.NodeTypes.Employee:
                {
                    properties =
                    "knaUid: " + '\n' +
                        "name: " + '\n' +
                        "phone: " + '\n' +
                        "email: ";
                    break;
                }
            case Node.NodeTypes.Company:
                {
                    properties =
                        "knaUid: " + '\n' +
                        "name: " + '\n' +
                        "orgNumber: " + '\n' +
                        "kind: " + '\n' +
                        "email: " + '\n' +
                        "phone: " + '\n' +
                        "street: " + '\n' +
                        "postalTown: " + '\n' +
                        "postalCode: ";
                    break;
                }
            case Node.NodeTypes.Application:
                {
                    properties =
                        "knaUid: " + '\n' +
                        "name: ";
                    break;
                }
            case Node.NodeTypes.Case:
                {
                    properties =
                        "knaUid: " + '\n' +
                        "name: ";
                    break;
                }
            case Node.NodeTypes.RealEstate:
                {
                    properties =
                        "knaUid: " + '\n' +
                        "name: " + '\n' +
                        "street: " + '\n' +
                        "postalTown: " + '\n' +
                        "postalCode: ";
                    break;
                }
            case Node.NodeTypes.Permit:
                {
                    properties =
                        "knaUid: " + '\n' +
                        "name: " + '\n' +
                        "kind: " + '\n' +
                        "granted: " + '\n' +
                        "grantedAt: " + '\n' +
                        "createdAt: " + '\n' +
                        "comment: " + '\n' +
                        "citizen: ";
                    break;
                }
            case Node.NodeTypes.Record:
                {
                    properties =
                        "knaUid: " + '\n' +
                        "name: " + '\n' +
                        "kind: " + '\n' +
                        "active: " + '\n' +
                        "registeredAt: " + '\n' +
                        "unregisteredAt: " + '\n' +
                        "createdAt: " + '\n' +
                        "comment: " + '\n' +
                        "citizen: ";
                    break;
                }
            case Node.NodeTypes.Registration:
                {
                    properties =
                        "knaUid: " + '\n' +
                        "name: " + '\n' +
                        "kind: " + '\n' +
                        "createdAt: " + '\n' +
                        "comment: " + '\n' +
                        "citizen: ";
                    break;
                }
            default:
                {
                    break;
                }
        }

        return properties;
    }
}