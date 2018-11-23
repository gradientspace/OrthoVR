using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using g3;
using f3;
using gs;
using gsbody;

namespace orthogen
{
    public class OGSerializer
    {

        public void StoreCurrent(string path)
        {
            DebugUtil.Log("[OGSerializer] Saving scene to " + path);

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;

            using (XmlWriter writer = XmlWriter.Create(path, settings)) {

                writer.WriteStartElement("OrthogenFile");

                XMLOutputStream stream = new XMLOutputStream(writer);
                StoreScene(stream, OG.Scene);

                StoreDataModel(writer);

                writer.WriteEndElement();
            }
        }



        public void RestoreToCurrent(string path)
        {
            DebugUtil.Log("[OGSerializer] Restoring scene from " + path);

            XmlDocument doc = new XmlDocument();
            try {
                doc.Load(path);
            }catch (Exception) {
                DebugUtil.Log("[OGSerializer] failed to read XmlDocument");
                throw;
            }

            // begin restore
            OGActions.BeginRestoreExistingScene();

            // restore scene objects
            XMLInputStream stream = new XMLInputStream() { xml = doc };
            SceneSerializer serializer = new SceneSerializer() {
                SOFactory = new SOFactory()
            };
            serializer.Restore(stream, OG.Scene);

            // restore datamodel
            RestoreDataModel(doc);
        }



        /// <summary>
        /// serialize scene, with SO filter
        /// </summary>
        protected virtual void StoreScene(IOutputStream o, FScene scene)
        {
            SceneSerializer serializer = new SceneSerializer();
            serializer.SOFilterF = SerializeSOFilter;
            serializer.Store(o, scene);
        }

        /// <summary>
        /// Serializer SO filter
        /// If you return false from this object for an SO, it is not serialized.
        /// </summary>
        protected virtual bool SerializeSOFilter(SceneObject so)
        {
            if (OG.Leg != null && (so == OG.Leg.SO || so == OG.Leg.RectifiedSO))
                return false;
            if (OG.Socket != null && (so == OG.Socket.Socket))
                return false;
            return true;
        }


        /// <summary>
        /// Serialize data model. THIS IS A DEMO!!
        /// </summary>
        protected virtual void StoreDataModel(XmlWriter writer)
        {
            writer.WriteStartElement("DataModel");

            if (OG.Leg != null) {

                // iterate over deformation operators and store them
                // elements are [so,op]
                foreach (var pair in OG.Leg.OperatorObjectPairs()) {
                    if ( pair.Item2 is EnclosedRegionOffsetOp ) {
                        EnclosedRegionOffsetOp op = pair.Item2 as EnclosedRegionOffsetOp;
                        writer.WriteStartElement("LegDeformOp");
                        writer.WriteAttributeString("OpType", op.GetType().ToString());
                        writer.WriteAttributeString("SceneObjectUUID", pair.Item1.UUID);
                        writer.WriteAttributeString("Offset", op.PushPullDistance.ToString());
                        writer.WriteEndElement();

                    } else if (pair.Item2 is EnclosedRegionSmoothOp) {
                        EnclosedRegionSmoothOp op = pair.Item2 as EnclosedRegionSmoothOp;
                        writer.WriteStartElement("LegDeformOp");
                        writer.WriteAttributeString("OpType", op.GetType().ToString());
                        writer.WriteAttributeString("SceneObjectUUID", pair.Item1.UUID);
                        writer.WriteAttributeString("Offset", op.OffsetDistance.ToString());
                        writer.WriteAttributeString("Smooth", op.SmoothAlpha.ToString());
                        writer.WriteEndElement();


                    } else if (pair.Item2 is PlaneBandExpansionOp) {
                        PlaneBandExpansionOp op = pair.Item2 as PlaneBandExpansionOp;
                        writer.WriteStartElement("LegDeformOp");
                        writer.WriteAttributeString("OpType", op.GetType().ToString());
                        writer.WriteAttributeString("SceneObjectUUID", pair.Item1.UUID);
                        writer.WriteAttributeString("Extent", op.BandDistance.ToString());
                        writer.WriteAttributeString("Offset", op.PushPullDistance.ToString());
                        writer.WriteAttributeString("Origin", op.Origin.ToString());
                        writer.WriteAttributeString("Normal", op.Normal.ToString());
                        writer.WriteEndElement();

                    } else if ( pair.Item2 is LengthenOp ) {
                        LengthenOp op = pair.Item2 as LengthenOp;
                        writer.WriteStartElement("LegDeformOp");
                        writer.WriteAttributeString("OpType", op.GetType().ToString());
                        writer.WriteAttributeString("SceneObjectUUID", pair.Item1.UUID);
                        writer.WriteAttributeString("Distance", op.LengthenDistance.ToString());
                        writer.WriteEndElement();
                    }
                }


                // what else?!?
            }

            writer.WriteEndElement();
        }



        /// <summary>
        /// parse the DataModel section of the save file, and restore the scene/datamodel as necessary
        /// </summary>
        /// <param name="xml"></param>
        protected virtual void RestoreDataModel(XmlDocument xml)
        {
            // look up root datamodel (should only be one)
            XmlNodeList datamodels = xml.SelectNodes("//DataModel");
            XmlNode datamodel = datamodels[0];


            // find scan 
            ScanSO scanSO = OG.Scene.FindSceneObjectsOfType<ScanSO>().FirstOrDefault();
            if (scanSO == null)
                throw new Exception("OGSerializer.RestoreDataModel: no ScanSO?");

            // [TODO] we have scanIn and scanOut, don't we?!?
            // start in scan state, restore the scan
            OGActions.RestoreSocketDesignFromScan(OG.Context, scanSO);

            
            // [TODO] should only do this transition if user has accepted scan
            //    (probably should have some current-state field in datamodel)
            OG.TransitionToState(RectifyState.Identifier);


            // restore LegModel deformation ops
            XmlNodeList deformationOps = datamodel.SelectNodes("LegDeformOp");
            foreach (XmlNode opNode in deformationOps) {
                string type = opNode.Attributes["OpType"].InnerText;
                string so_uuid = opNode.Attributes["SceneObjectUUID"].InnerText;

                if ( type == typeof(EnclosedRegionOffsetOp).ToString() ) {
                    EnclosedPatchSO patchSO = OG.Scene.FindByUUID(so_uuid) as EnclosedPatchSO;
                    var newOp = OGActions.AddNewRegionDeformation(patchSO, LegModel.LegDeformationTypes.Offset);
                    double offset = 0.0f;
                    if (double.TryParse(opNode.Attributes["Offset"].InnerText, out offset))
                        (newOp as EnclosedRegionOffsetOp).PushPullDistance = offset;


                } else if ( type == typeof(EnclosedRegionSmoothOp).ToString() ) {
                    EnclosedPatchSO patchSO = OG.Scene.FindByUUID(so_uuid) as EnclosedPatchSO;
                    var newOp = OGActions.AddNewRegionDeformation(patchSO, LegModel.LegDeformationTypes.Smooth);
                    double smooth = 0.0f;
                    if (double.TryParse(opNode.Attributes["Smooth"].InnerText, out smooth))
                        (newOp as EnclosedRegionSmoothOp).SmoothAlpha = smooth;
                    double offset = 0.0f;
                    if (double.TryParse(opNode.Attributes["Offset"].InnerText, out offset))
                        (newOp as EnclosedRegionSmoothOp).OffsetDistance = offset;


                } else if (type == typeof(PlaneBandExpansionOp).ToString()) {
                    PlaneIntersectionCurveSO curveSO = OG.Scene.FindByUUID(so_uuid) as PlaneIntersectionCurveSO;
                    var newOp = OGActions.AddNewPlaneBandExpansion(curveSO);
                    double extent = 0.0f;
                    if (double.TryParse(opNode.Attributes["Extent"].InnerText, out extent))
                        (newOp as PlaneBandExpansionOp).BandDistance = extent;
                    double offset = 0.0f;
                    if (double.TryParse(opNode.Attributes["Offset"].InnerText, out offset))
                        (newOp as PlaneBandExpansionOp).PushPullDistance = offset;
                    Vector3d origin = TryParseVector3(opNode.Attributes["Origin"].InnerText);
                    (newOp as PlaneBandExpansionOp).Origin = origin;
                    Vector3d normal = TryParseVector3(opNode.Attributes["Normal"].InnerText);
                    (newOp as PlaneBandExpansionOp).Normal = normal;


                } else if (type == typeof(LengthenOp).ToString() ) {
                    LengthenPivotSO pivotSO = OG.Scene.FindByUUID(so_uuid) as LengthenPivotSO;
                    LengthenOp newOp = OGActions.AddNewLengthenOp(pivotSO);
                    double offset = 0.0f;
                    if (double.TryParse(opNode.Attributes["Distance"].InnerText, out offset))
                        newOp.LengthenDistance = offset;
                }
            }


            // if we have a trimloop, restore it
            TrimLoopSO trimSO = OG.Scene.FindSceneObjectsOfType<TrimLoopSO>().FirstOrDefault();
            if ( trimSO != null ) {
                OG.TransitionToState(SocketDesignState.Identifier);
                OGActions.AddNewTrimCurve(trimSO);
            }

            // [TODO] restore socket enabled, parameters, etc

        }





        protected Vector3d TryParseVector3(string text)
        {
            if ( text == null || text.Length == 0 )
                throw new Exception("OGSerializer.ParseVector3: invalid input");
            Vector3d v = Vector3d.Zero;
            string[] tokens = text.Split(' ');
            if (tokens.Length != 3)
                throw new Exception("OGSerializer.ParseVector3: string [" + text + "] is not a 3-element vector");
            if (double.TryParse(tokens[0], out v.x) &&
                 double.TryParse(tokens[1], out v.y) &&
                 double.TryParse(tokens[2], out v.z))
                return v;
            return Vector3d.Zero;
        }


    }









}
